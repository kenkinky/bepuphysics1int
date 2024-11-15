﻿using BEPUphysics.CollisionRuleManagement;
using BEPUphysics.Constraints.TwoEntity.Joints;
using BEPUphysics.Entities;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics.NarrowPhaseSystems;
using BEPUutilities;
using FixMath.NET;

namespace BEPUfloatBenchmark.Benchmarks
{
    public class SelfCollidingClothBenchmark : Benchmark
    {
        protected override void InitializeSpace()
        {
            //Joints can also act like springs by modifying their springSettings.
            //Though using a bunch of DistanceJoint objects can be slower than just applying direct spring forces,
            //it is significantly more stable and allows rigid structures.
            //The extra stability can make it useful for cloth-like simulations.
            Entity latticePiece;
            BallSocketJoint joint;

            NarrowPhaseHelper.Factories.BoxBox.Count = 4000;
            NarrowPhaseHelper.Factories.BoxSphere.Count = 1000;

            int numColumns = 40;
            int numRows = 40;
            float xSpacing = 1.0f;
            float zSpacing = 1.0f;
            var lattice = new Entity[numRows, numColumns];
            for (int i = 0; i < numRows; i++)
            for (int j = 0; j < numColumns; j++)
            {
                latticePiece = new Box(
                    new Vector3(
                        (FP)(xSpacing * i - (numRows - 1) * xSpacing / 2),
                        15.58m,
                        (FP)(2 + zSpacing * j - (numColumns - 1) * zSpacing / 2)),
                    (FP)xSpacing, (FP).2f, (FP)zSpacing, 10);

                lattice[i, j] = latticePiece;

                Space.Add(latticePiece);
            }

            //The joints composing the cloth can have their max iterations set independently from the solver iterations.
            //More iterations (up to the solver's own max) will increase the quality at the cost of speed.
            int clothIterations = 3;
            //So while the above clamps joint iterations, setting the solver's iteration limit can lower the
            //rest of the solving load (collisions).
            Space.Solver.IterationLimit = 10;

            float damping = 20000, stiffness = 20000;
            float starchDamping = 5000, starchStiffness = 500;

            //Loop through the grid and set up the joints.
            for (int i = 0; i < numRows; i++)
            for (int j = 0; j < numColumns; j++)
            {
                if (i == 0 && j + 1 < numColumns)
                {
                    //Add in column connections for left edge.
                    joint = new BallSocketJoint(lattice[0, j], lattice[0, j + 1], lattice[0, j].Position + new Vector3((FP)(-xSpacing / 2), 0, (FP)(zSpacing / 2)));
                    joint.SpringSettings.Damping = (FP)damping;
                    joint.SpringSettings.Stiffness = (FP)stiffness;
                    joint.SolverSettings.MaximumIterationCount = clothIterations;
                    Space.Add(joint);
                }

                if (i == numRows - 1 && j + 1 < numColumns)
                {
                    //Add in column connections for right edge.
                    joint = new BallSocketJoint(lattice[numRows - 1, j], lattice[numRows - 1, j + 1], lattice[numRows - 1, j].Position + new Vector3((FP)(xSpacing / 2), 0, (FP)(zSpacing / 2)));
                    joint.SpringSettings.Damping = (FP)damping;
                    joint.SpringSettings.Stiffness = (FP)stiffness;
                    joint.SolverSettings.MaximumIterationCount = clothIterations;
                    Space.Add(joint);
                }

                if (i + 1 < numRows && j == 0)
                {
                    //Add in row connections for top edge.
                    joint = new BallSocketJoint(lattice[i, 0], lattice[i + 1, 0], lattice[i, 0].Position + new Vector3((FP)(xSpacing / 2), 0, (FP)(-zSpacing / 2)));
                    joint.SpringSettings.Damping = (FP)damping;
                    joint.SpringSettings.Stiffness = (FP)stiffness;
                    joint.SolverSettings.MaximumIterationCount = clothIterations;
                    Space.Add(joint);
                }

                if (i + 1 < numRows && j == numColumns - 1)
                {
                    //Add in row connections for bottom edge.
                    joint = new BallSocketJoint(lattice[i, numColumns - 1], lattice[i + 1, numColumns - 1], lattice[i, numColumns - 1].Position + new Vector3((FP)(xSpacing / 2), 0, (FP)(zSpacing / 2)));
                    joint.SpringSettings.Damping = (FP)damping;
                    joint.SpringSettings.Stiffness = (FP)stiffness;
                    joint.SolverSettings.MaximumIterationCount = clothIterations;
                    Space.Add(joint);
                }


                if (i + 1 < numRows && j + 1 < numColumns)
                {
                    //Add in interior connections.
                    joint = new BallSocketJoint(lattice[i, j], lattice[i + 1, j], lattice[i, j].Position + new Vector3((FP)(xSpacing / 2), 0, (FP)(zSpacing / 2)));
                    joint.SpringSettings.Damping = (FP)damping;
                    joint.SpringSettings.Stiffness = (FP)stiffness;
                    joint.SolverSettings.MaximumIterationCount = clothIterations;
                    Space.Add(joint);

                    joint = new BallSocketJoint(lattice[i, j], lattice[i, j + 1], lattice[i, j].Position + new Vector3((FP)(xSpacing / 2), 0, (FP)(zSpacing / 2)));
                    joint.SpringSettings.Damping = (FP)damping;
                    joint.SpringSettings.Stiffness = (FP)stiffness;
                    joint.SolverSettings.MaximumIterationCount = clothIterations;
                    Space.Add(joint);

                    joint = new BallSocketJoint(lattice[i, j], lattice[i + 1, j + 1], lattice[i, j].Position + new Vector3((FP)(xSpacing / 2), 0, (FP)(zSpacing / 2)));
                    joint.SpringSettings.Damping = (FP)damping;
                    joint.SpringSettings.Stiffness = (FP)stiffness;
                    joint.SolverSettings.MaximumIterationCount = clothIterations;
                    Space.Add(joint);
                }

                if (i + 2 < numRows && j + 2 < numColumns)
                {
                    //Add in skipping 'starch' connections.
                    joint = new BallSocketJoint(lattice[i, j], lattice[i + 2, j], lattice[i, j].Position + new Vector3((FP)xSpacing, 0, (FP)zSpacing));
                    joint.SpringSettings.Damping = (FP)starchDamping;
                    joint.SpringSettings.Stiffness = (FP)starchStiffness;
                    joint.SolverSettings.MaximumIterationCount = clothIterations;
                    Space.Add(joint);

                    joint = new BallSocketJoint(lattice[i, j], lattice[i, j + 2], lattice[i, j].Position + new Vector3((FP)xSpacing, 0, (FP)zSpacing));
                    joint.SpringSettings.Damping = (FP)starchDamping;
                    joint.SpringSettings.Stiffness = (FP)starchStiffness;
                    joint.SolverSettings.MaximumIterationCount = clothIterations;
                    Space.Add(joint);

                    joint = new BallSocketJoint(lattice[i, j], lattice[i + 2, j + 2], lattice[i, j].Position + new Vector3((FP)xSpacing, 0, (FP)zSpacing));
                    joint.SpringSettings.Damping = (FP)starchDamping;
                    joint.SpringSettings.Stiffness = (FP)starchStiffness;
                    joint.SolverSettings.MaximumIterationCount = clothIterations;
                    Space.Add(joint);
                }

                //Add in collision rules.
                if (j - 1 >= 0)
                {
                    if (i - 1 >= 0) CollisionRules.AddRule(lattice[i, j], lattice[i - 1, j - 1], CollisionRule.NoBroadPhase);
                    CollisionRules.AddRule(lattice[i, j], lattice[i, j - 1], CollisionRule.NoBroadPhase);
                    if (i + 1 < numRows) CollisionRules.AddRule(lattice[i, j], lattice[i + 1, j - 1], CollisionRule.NoBroadPhase);
                }

                if (i + 1 < numRows) CollisionRules.AddRule(lattice[i, j], lattice[i + 1, j], CollisionRule.NoBroadPhase);
            }


            //Add some ground.
            var sphere = new Sphere(new Vector3(7, 0, 0), 10);
            sphere.Material.KineticFriction = .2m;
            Space.Add(sphere);
            Space.Add(new Box(new Vector3(0, -20.5m, 0), 100, 10, 100));
        }
    }
}