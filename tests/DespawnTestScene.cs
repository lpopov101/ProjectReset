using System.Collections.Generic;
using Godot;

// Headless harness for Pool despawn safety.
//
//   Godot --path . --headless res://tests/despawn_test_scene.tscn \
//         --quit-after 600 -- <scenario>
//
// Scenarios:
//   direct   (default) two Despawn() calls on one live projectile in the same
//            frame, which is exactly what two BodyEntered signals produce.
//   multihit a projectile driven into two static bodies so the physics server
//            reports both contacts in a single step.
//   reuse    plain spawn/despawn cycles, to prove a despawn guard does not also
//            block legitimate re-despawns of a recycled instance.
//
// The invariant under test: a pool never hands the same instance to two callers
// at once. A projectile despawned twice must be parked in the pool exactly once.
public partial class DespawnTestScene : Node3D
{
    private const int PROBE_SPAWNS = 5;
    private const int REUSE_CYCLES = 3;
    private const int COLLISION_TIMEOUT_FRAMES = 240;

    private Pool _pool;
    private string _scenario = "direct";
    private int _frame = 0;
    private int _step = 0;
    private int _cycle = 0;
    private bool _finished = false;

    private DespawnTestProjectile _subject;
    private readonly List<ulong> _cycleIds = new List<ulong>();
    private readonly List<DespawnTestProjectile> _probes = new List<DespawnTestProjectile>();

    public override void _Ready()
    {
        base._Ready();
        var userArgs = OS.GetCmdlineUserArgs();
        if (userArgs.Length > 0)
        {
            _scenario = userArgs[0];
        }

        // A pool of one makes "reuse" deterministic: every spawn must return the
        // single parked instance. The others need a few spares so a duplicated
        // entry surfaces after only a handful of probe spawns.
        var poolSize = _scenario == "reuse" ? 1 : 4;
        var template = GD.Load<PackedScene>("res://tests/despawn_test_projectile.tscn");
        _pool = Pool.Create(template, poolSize);

        // Added as a child so Pool._Process (and its despawn drain) runs after
        // this node's _Process in the same frame.
        AddChild(_pool);

        if (_scenario == "multihit")
        {
            buildMultiHitGeometry();
        }

        GD.Print($"DESPAWNTEST scenario={_scenario} pool_size={poolSize}");
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (_finished)
        {
            return;
        }
        _frame++;
        if (_scenario == "direct")
        {
            stepDirect();
        }
        else if (_scenario == "multihit")
        {
            stepMultiHit();
        }
        else if (_scenario == "reuse")
        {
            stepReuse();
        }
        else
        {
            fail($"unknown scenario '{_scenario}'");
        }
    }

    private void stepDirect()
    {
        if (_step == 0)
        {
            _subject = spawn(Vector3.Zero);
            GD.Print($"DESPAWNTEST f={_frame} spawned id={_subject.GetInstanceId()}");
            _step = 1;
        }
        else if (_step == 1)
        {
            GD.Print($"DESPAWNTEST f={_frame} despawn x2 id={_subject.GetInstanceId()}");
            ((ISpawnable)_subject).Despawn();
            ((ISpawnable)_subject).Despawn();
            _step = 2;
        }
        else if (_step == 2)
        {
            // Both queued despawns drained at the end of the previous frame.
            probeAndReport();
        }
    }

    private void stepMultiHit()
    {
        if (_step == 0)
        {
            _subject = spawn(new Vector3(0, 0, 4));
            GD.Print($"DESPAWNTEST f={_frame} fired id={_subject.GetInstanceId()}");
            _step = 1;
        }
        else if (_step == 1)
        {
            if (_subject.CollideCount > 0)
            {
                GD.Print(
                    $"DESPAWNTEST f={_frame} contacts={_subject.CollideCount} "
                        + $"z={_subject.GlobalPosition.Z:F3}"
                );
                if (_subject.CollideCount < 2)
                {
                    inconclusive(
                        $"only {_subject.CollideCount} contact reported in one step, "
                            + "so no double despawn was requested this run"
                    );
                    return;
                }
                _step = 2;
            }
            else if (_frame > COLLISION_TIMEOUT_FRAMES)
            {
                inconclusive($"projectile never collided (z={_subject.GlobalPosition.Z:F3})");
            }
        }
        else if (_step == 2)
        {
            probeAndReport();
        }
    }

    private void stepReuse()
    {
        if (_step == 0)
        {
            _subject = spawn(Vector3.Zero);
            _cycleIds.Add(_subject.GetInstanceId());
            GD.Print(
                $"DESPAWNTEST f={_frame} cycle={_cycle} spawned id={_subject.GetInstanceId()}"
            );
            _step = 1;
        }
        else if (_step == 1)
        {
            ((ISpawnable)_subject).Despawn();
            GD.Print($"DESPAWNTEST f={_frame} cycle={_cycle} despawned");
            _step = 2;
        }
        else if (_step == 2)
        {
            _cycle++;
            if (_cycle >= REUSE_CYCLES)
            {
                reportReuse();
            }
            else
            {
                _step = 0;
            }
        }
    }

    // Drains the pool and looks for an instance that came back more than once.
    private void probeAndReport()
    {
        for (int i = 0; i < PROBE_SPAWNS; i++)
        {
            _probes.Add(spawn(new Vector3(i * 5F, 0, 0)));
        }

        var counts = new Dictionary<ulong, int>();
        var ids = new List<string>();
        foreach (var probe in _probes)
        {
            var id = probe.GetInstanceId();
            counts[id] = counts.ContainsKey(id) ? counts[id] + 1 : 1;
            ids.Add(id.ToString());
        }
        GD.Print($"DESPAWNTEST f={_frame} probe_ids=[{string.Join(", ", ids)}]");

        foreach (var pair in counts)
        {
            if (pair.Value > 1)
            {
                fail($"pool handed instance {pair.Key} to {pair.Value} callers at once");
                return;
            }
        }
        pass($"{PROBE_SPAWNS} spawns returned {PROBE_SPAWNS} distinct instances");
    }

    private void reportReuse()
    {
        var ids = new List<string>();
        foreach (var id in _cycleIds)
        {
            ids.Add(id.ToString());
        }
        GD.Print($"DESPAWNTEST f={_frame} cycle_ids=[{string.Join(", ", ids)}]");

        foreach (var id in _cycleIds)
        {
            if (id != _cycleIds[0])
            {
                fail("a spawn/despawn cycle did not recycle: the pool of one ran dry");
                return;
            }
        }
        pass($"{REUSE_CYCLES} spawn/despawn cycles recycled the same instance");
    }

    private DespawnTestProjectile spawn(Vector3 position)
    {
        return _pool.SpawnAsChild3D<DespawnTestProjectile>(this, position, Vector3.Zero);
    }

    // Two slabs with a 0.5 gap between them. A sphere of radius 0.5 driven down
    // the gap meets both front faces in the same physics step, so both
    // BodyEntered signals fire before either despawn is applied.
    private void buildMultiHitGeometry()
    {
        addStaticBox(new Vector3(-0.75F, 0, 0), new Vector3(1, 2, 1));
        addStaticBox(new Vector3(0.75F, 0, 0), new Vector3(1, 2, 1));
    }

    private void addStaticBox(Vector3 position, Vector3 size)
    {
        var staticBody = new StaticBody3D();
        staticBody.Position = position;
        var collisionShape = new CollisionShape3D();
        collisionShape.Shape = new BoxShape3D { Size = size };
        staticBody.AddChild(collisionShape);
        AddChild(staticBody);
    }

    private void pass(string reason)
    {
        finish("PASS", reason, 0);
    }

    private void fail(string reason)
    {
        finish("FAIL", reason, 1);
    }

    private void inconclusive(string reason)
    {
        finish("INCONCLUSIVE", reason, 2);
    }

    private void finish(string status, string reason, int exitCode)
    {
        _finished = true;
        GD.Print($"DESPAWNTEST RESULT scenario={_scenario} status={status} reason={reason}");
        GetTree().Quit(exitCode);
    }
}
