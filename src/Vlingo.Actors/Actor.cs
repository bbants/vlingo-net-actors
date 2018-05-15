﻿using System;
using Vlingo.Actors.TestKit;

namespace Vlingo.Actors
{
    public abstract class Actor : IStartable, IStoppable
    {
        public LifeCycle LifeCycle { get; }

        public Address Address => LifeCycle.Address;

        public DeadLetters DeadLetters => LifeCycle.Environment.Stage.World.DeadLetters;

        public virtual void Start()
        {
        }

        public bool IsStopped => LifeCycle.IsStopped;

        public void Stop()
        {
            if (!IsStopped)
            {
                if (LifeCycle.Address.Id != World.DeadlettersId)
                {
                    LifeCycle.Stop(this);
                }
            }
        }

        public virtual TestState ViewTestState() => new TestState();

        public override bool Equals(object other)
        {
            if (other == null || other.GetType() != GetType())
            {
                return false;
            }

            return Address.Equals(((Actor) other).LifeCycle.Address);
        }

        public override int GetHashCode() => LifeCycle.GetHashCode();

        public override string ToString() => $"Actor[type={GetType().Name} address={Address}]";

        protected Actor()
        {
            var maybeEnvironment = ActorFactory.ThreadLocalEnvironment.Value;
            LifeCycle = new LifeCycle(maybeEnvironment ?? new TestEnvironment());
            ActorFactory.ThreadLocalEnvironment.Value = null;
        }

        protected T ChildActorFor<T>(Definition definition)
        {
            if (definition.Supervisor != null)
            {
                return LifeCycle.Environment.Stage.ActorFor<T>(definition, this, definition.Supervisor,
                    Logger);
            }
            else
            {
                if (this is ISupervisor)
                {
                    return LifeCycle.Environment.Stage.ActorFor<T>(
                        definition, 
                        this,
                        LifeCycle.LookUpProxy(typeof(ISupervisor)),
                        Logger);
                }
                else
                {
                    return LifeCycle.Environment.Stage.ActorFor<T>(definition, this, null, Logger);
                }
            }
        }

        protected Definition Definition => LifeCycle.Definition;

        protected Logger Logger => LifeCycle.Environment.Logger;

        protected Actor Parent
        {
            get
            {
                if (LifeCycle.Environment.IsSecured)
                {
                    throw new InvalidOperationException("A secured actor cannot provide its parent.");
                }

                return LifeCycle.Environment.Parent;
            }
        }

        protected void Secure()
        {
            LifeCycle.Secure();
        }

        internal T SelfAs<T>()
        {
            return LifeCycle.Environment.Stage.ActorProxyFor<T>(this, LifeCycle.Environment.Mailbox);
        }

        protected IOutcomeInterest<TOutcome> SelfAsOutcomeInterest<TOutcome, TRef>(TRef reference)
        {
            var outcomeAware = LifeCycle.Environment.Stage.ActorProxyFor<IOutcomeAware<TOutcome, TRef>>(
                this,
                LifeCycle.Environment.Mailbox);

            return new OutcomeInterestActorProxy<TOutcome, TRef>(outcomeAware, reference);
        }

        protected Stage Stage
        {
            get
            {
                if (LifeCycle.Environment.IsSecured)
                {
                    throw new InvalidOperationException("A secured actor cannot provide its stage.");
                }

                return LifeCycle.Environment.Stage;
            }
        }

        protected Stage StageNamed(string name)
        {
            return LifeCycle.Environment.Stage.World.StageNamed(name);
        }


        protected bool IsDispersing =>
            LifeCycle.Environment.Stowage.IsDispersing;


        protected void DisperseStowedMessages()
        {
            LifeCycle.Environment.Stowage.DispersingMode();
        }

        protected bool IsStowing =>
            LifeCycle.Environment.Stowage.IsStowing;


        protected void StowMessages()
        {
            LifeCycle.Environment.Stowage.StowingMode();
        }

        //=======================================
        // life cycle overrides
        //=======================================

        protected virtual void BeforeStart()
        {
            // override
        }

        protected virtual void AfterStop()
        {
            // override
        }

        protected virtual void BeforeRestart(Exception reason)
        {
            // override
            LifeCycle.AfterStop(this);
        }

        protected virtual void AfterRestart(Exception reason)
        {
            // override
            LifeCycle.BeforeStart(this);
        }

        protected virtual void BeforeResume(Exception reason)
        {
            // override
        }
    }
}