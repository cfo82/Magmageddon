using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System.Text;

namespace ProjectMagma.Simulation.Collision
{
    class TestList
    {
        private interface ChangeEntry
        {
            void ApplyChange();
        }

        private class AddEntry : ChangeEntry
        {
            public AddEntry(
                TestList testList,
                CollisionEntity entity
            )
            {
                this.TestList = testList;
                this.Entity = entity;
            }

            public void ApplyChange()
            {
                TestList.Apply(this);
            }

            public readonly TestList TestList;
            public readonly CollisionEntity Entity;
        }

        private class RemoveEntry : ChangeEntry
        {
            public RemoveEntry(
                TestList testList,
                CollisionEntity entity
            )
            {
                this.TestList = testList;
                this.Entity = entity;
            }

            public void ApplyChange()
            {
                TestList.Apply(this);
            }

            public readonly TestList TestList;
            public readonly CollisionEntity Entity;
        }

        public class TestEntry
        {
            public TestEntry(
                CollisionEntity entityA,
                CollisionEntity entityB
            )
            {
                this.EntityA = entityA;
                this.EntityB = entityB;
            }

            public readonly CollisionEntity EntityA;
            public readonly CollisionEntity EntityB;
        }

        public TestList()
        {
            this.changeList = new List<ChangeEntry>();
            this.entityList = new List<CollisionEntity>();
            this.collisionList = new List<TestEntry>();
            this.currentCollisionEntry = 0;
            this.inCollisionDetection = 0;
        }

        public void Add(CollisionEntity entity)
        {
            Debug.Assert(inCollisionDetection == 0);
            changeList.Add(new AddEntry(this, entity));
        }

        public bool ContainsCollisionEntity(CollisionProperty property)
        {
            Debug.Assert(inCollisionDetection == 0);
            for (int i = 0; i < entityList.Count; ++i)
            {
                if (entityList[i].CollisionProperty == property)
                {
                    return true;
                }
            }

            return false;
        }

        public void Remove(CollisionEntity entity)
        {
            Debug.Assert(inCollisionDetection == 0);
            changeList.Add(new RemoveEntry(this, entity));
        }

        public void Remove(CollisionProperty property)
        {
            Debug.Assert(inCollisionDetection == 0);
            for (int i = 0; i < entityList.Count; ++i)
            {
                if (entityList[i].CollisionProperty == property)
                {
                    Remove(entityList[i]);
                    return;
                }
            }
        }

        public CollisionEntity GetCollisionEntity(Entity entity)
        {
            foreach (CollisionEntity collisionEntity in entityList)
            {
                if (collisionEntity.Entity == entity)
                {
                    return collisionEntity;
                }
            }
            return null;
        }

        void Apply(AddEntry entry)
        {
            foreach (CollisionEntity entity in entityList)
            {
                collisionList.Add(new TestEntry(entry.Entity, entity));
            }

            entityList.Add(entry.Entity);
        }

        void Apply(RemoveEntry entry)
        {
            for (int i = 0; i < collisionList.Count; ++i)
            {
                if (collisionList[i].EntityA == entry.Entity ||
                    collisionList[i].EntityB == entry.Entity)
                {
                    collisionList.RemoveAt(i);
                    --i;
                }
            }

            entityList.Remove(entry.Entity);
        }

        public void BeginCollisionDetection()
        {
            Debug.Assert(inCollisionDetection == 0);

            foreach (ChangeEntry entry in changeList)
            {
                entry.ApplyChange();
            }
            changeList.Clear();

            Interlocked.Exchange(ref inCollisionDetection, 1);
            Interlocked.Exchange(ref currentCollisionEntry, -1);
        }

        public TestEntry GetNextCollisionEntry()
        {
            Debug.Assert(inCollisionDetection == 1);
            long index = Interlocked.Increment(ref currentCollisionEntry);

            if (index < collisionList.Count)
            {
                return collisionList[(int)index];
            }
            else
            {
                return null;
            }
        }

        public void EndCollisionDetection()
        {
            Debug.Assert(inCollisionDetection == 1);
            Interlocked.Exchange(ref inCollisionDetection, 0);
        }

        private List<ChangeEntry> changeList;
        private List<CollisionEntity> entityList;
        private List<TestEntry> collisionList;
        private int currentCollisionEntry;
        private int inCollisionDetection;
    }
}
