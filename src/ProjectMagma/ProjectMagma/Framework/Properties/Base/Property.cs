
namespace ProjectMagma.Framework
{
    public delegate void ActivationStateChangedHandler(Property property);
    public delegate void DeactivationHandler();

    public abstract class Property
    {
        public Property()
        {
            isActive = false;
        }

        public void Activate()
        {
            if (!isActive)
            {
                isActive = true;
                if (OnActivated != null)
                {
                    OnActivated(this);
                }
            }
        }

        public void Deactivate()
        {
            if (isActive)
            {
                isActive = false;
                if (OnDeactivated != null)
                {
                    OnDeactivated(this);
                }
            }
        }

        public bool IsActive
        {
            set
            {
                if (value)
                {
                    Activate();
                }
                else
                {
                    Deactivate();
                }
            }

            get
            {
                return isActive;
            }
        }

        public event ActivationStateChangedHandler OnActivated;
        public event ActivationStateChangedHandler OnDeactivated;

        public abstract void OnAttached(AbstractEntity entity);
        public abstract void OnDetached(AbstractEntity entity);

        private bool isActive;
    }
}
