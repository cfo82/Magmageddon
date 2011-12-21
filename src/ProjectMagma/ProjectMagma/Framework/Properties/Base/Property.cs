
namespace ProjectMagma.Framework
{
    public delegate void ActivationStateChangedHandler(Property property, bool isActive);
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
                if (OnActiveStateChanged != null)
                {
                    OnActiveStateChanged(this, isActive);
                }
            }
        }

        public void Deactivate()
        {
            if (isActive)
            {
                isActive = false;
                if (OnActiveStateChanged != null)
                {
                    OnActiveStateChanged(this, isActive);
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

        public event ActivationStateChangedHandler OnActiveStateChanged;

        public abstract void OnAttached(AbstractEntity entity);
        public abstract void OnDetached(AbstractEntity entity);

        private bool isActive;
    }
}
