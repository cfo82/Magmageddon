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
            this.entityList = new List<CollisionEntity>();
            this.collisionList = new List<TestEntry>();
            this.currentCollisionEntry = 0;
            this.inCollisionDetection = 0;
        }

        public void Add(CollisionEntity entity)
        {
            Debug.Assert(inCollisionDetection == 0);

            foreach (CollisionEntity otherEntity in entityList)
            {
                collisionList.Add(new TestEntry(entity, otherEntity));
            }

            entityList.Add(entity);
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
         
            for (int i = 0; i < collisionList.Count; ++i)
            {
                if (collisionList[i].EntityA == entity ||
                    collisionList[i].EntityB == entity)
                {
                    collisionList.RemoveAt(i);
                    --i;
                }
            }

            entityList.Remove(entity);
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

        public void BeginCollisionDetection()
        {
            Debug.Assert(inCollisionDetection == 0);

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

        private List<CollisionEntity> entityList;
        private List<TestEntry> collisionList;
        private int currentCollisionEntry;
        private int inCollisionDetection;
    }
}
