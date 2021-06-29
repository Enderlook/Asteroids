using Enderlook.GOAP;

using System;

namespace Asteroids.Entities.Enemies
{
    public sealed partial class Boss
    {
        public delegate bool CheckAndTrySatisfy(ref BossState worldState);
        public delegate SatisfactionResult CheckAndTrySatisfy2(BossState before, ref BossState now);
        public delegate void ApplyEffect<T>(T parameter, ref BossState worldState);
        public delegate void ApplyEffect(ref BossState worldState);

        private struct Unit { }

        private abstract class NodeBase : IGoal<BossState>, IAction<BossState, IGoal<BossState>>
        {
            // Adapter to use lambdas in GOAP.

            private readonly CheckAndTrySatisfy2 checkAndTrySatisfy2;
            private readonly CheckAndTrySatisfy checkAndTrySatisfy;

            protected bool HasGoal => !(checkAndTrySatisfy2 is null);

            protected NodeBase(CheckAndTrySatisfy2 checkAndTrySatisfy2, CheckAndTrySatisfy checkAndTrySatisfy)
            {
                this.checkAndTrySatisfy2 = checkAndTrySatisfy2;
                this.checkAndTrySatisfy = checkAndTrySatisfy;
                if (checkAndTrySatisfy is null && !(checkAndTrySatisfy2 is null))
                    checkAndTrySatisfy = (ref BossState worldState) => checkAndTrySatisfy2(worldState, ref worldState) == SatisfactionResult.Satisfied;
            }

            public abstract void Visit<TActionHandleAcceptor>(ref TActionHandleAcceptor acceptor, BossState worldState)
                where TActionHandleAcceptor : IActionHandleAcceptor<BossState, IGoal<BossState>>;

            SatisfactionResult IGoal<BossState>.CheckAndTrySatisfy(BossState before, ref BossState now)
                => checkAndTrySatisfy2(before, ref now);

            bool IGoal<BossState>.CheckAndTrySatisfy(ref BossState worldState)
                => checkAndTrySatisfy(ref worldState);
        }

        private sealed class Node : NodeBase
        {
            private readonly ApplyEffect applyEffect;
            private readonly Func<bool> checkProceduralPreconditions;
            private readonly Func<float> getCost;

            private static readonly Func<bool> alwaysTrue = () => true;

            public Node(
                CheckAndTrySatisfy2 checkAndTrySatisfy2, CheckAndTrySatisfy checkAndTrySatisfy,
                ApplyEffect applyEffect, Func<bool> checkProceduralPreconditions,
                Func<float> getCostAndRequiredGoal
                ) : base(checkAndTrySatisfy2, checkAndTrySatisfy)
            {
                this.applyEffect = applyEffect;
                this.checkProceduralPreconditions = checkProceduralPreconditions ?? alwaysTrue;
                getCost = getCostAndRequiredGoal;
            }

            public override void Visit<TActionHandleAcceptor>(ref TActionHandleAcceptor acceptor, BossState worldState)
                => acceptor.Accept(new Handle(this));

            private readonly struct Handle : IActionHandle<BossState, IGoal<BossState>>
            {
                private readonly Node node;

                public Handle(Node node) => this.node = node;

                void IActionHandle<BossState, IGoal<BossState>>.ApplyEffect(ref BossState worldState)
                    => node.applyEffect(ref worldState);

                bool IActionHandle<BossState, IGoal<BossState>>.CheckProceduralPreconditions()
                    => node.checkProceduralPreconditions();

                bool IActionHandle<BossState, IGoal<BossState>>.GetCostAndRequiredGoal(out float cost, out IGoal<BossState> goal)
                {
                    cost = node.getCost();
                    goal = node;
                    return node.HasGoal;
                }
            }
        }

        private sealed class Node<T> : NodeBase
        {
            private readonly ApplyEffect<T> applyEffect;
            private readonly Func<T, bool> checkProceduralPreconditions;
            private readonly Func<T, float> getCost;
            private readonly Func<BossState, T> getParameter;

            private static readonly Func<T, bool> alwaysTrue = _ => true;

            public Node(
                CheckAndTrySatisfy2 checkAndTrySatisfy2, CheckAndTrySatisfy checkAndTrySatisfy,
                Func<BossState, T> getParameter,
                ApplyEffect<T> applyEffect, Func<T, bool> checkProceduralPreconditions,
                Func<T, float> getCostAndRequiredGoal
                ) : base(checkAndTrySatisfy2, checkAndTrySatisfy)
            {
                this.applyEffect = applyEffect;
                this.getParameter = getParameter;
                this.checkProceduralPreconditions = checkProceduralPreconditions ?? alwaysTrue;
                getCost = getCostAndRequiredGoal;
            }

            public override void Visit<TActionHandleAcceptor>(ref TActionHandleAcceptor acceptor, BossState worldState)
                => acceptor.Accept(new Handle(this, getParameter(worldState)));

            private readonly struct Handle : IActionHandle<BossState, IGoal<BossState>>
            {
                private readonly Node<T> node;
                private readonly T parameter;

                public Handle(Node<T> node, T parameter)
                {
                    this.node = node;
                    this.parameter = parameter;
                }

                void IActionHandle<BossState, IGoal<BossState>>.ApplyEffect(ref BossState worldState)
                    => node.applyEffect(parameter, ref worldState);

                bool IActionHandle<BossState, IGoal<BossState>>.CheckProceduralPreconditions()
                    => node.checkProceduralPreconditions(parameter);

                bool IActionHandle<BossState, IGoal<BossState>>.GetCostAndRequiredGoal(out float cost, out IGoal<BossState> goal)
                {
                    cost = node.getCost(parameter);
                    goal = node;
                    return node.HasGoal;
                }
            }
        }
    }
}