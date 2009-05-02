using Microsoft.Xna.Framework.Content;

namespace ProjectMagma
{
    public class WrappedContentManager
    {
        public WrappedContentManager(ContentManager manager)
        {
            this.manager = manager;
        }


        /// <summary>
        ///    Loads an asset that has been processed by the Content Pipeline. Reference
        ///    page contains code sample.
        /// </summary>
        /// <typeparam name="T">
        /// </typeparam>
        /// <param name="assetName">
        ///     Asset name, relative to the loader root directory, and not including the
        ///     .xnb file extension.
        /// </param>
        /// <returns>
        ///     The loaded asset. Repeated calls to load the same asset will return the same
        ///     object instance.
        /// </returns>
        public virtual T Load<T>(string assetName)
        {
            lock (manager)
            {
                return manager.Load<T>(assetName);
            }
        }

        public string RootDirectory
        {
            get
            {
                lock (manager)
                {
                    return manager.RootDirectory;
                }
            }

            set
            {
                lock (manager)
                {
                    manager.RootDirectory = value;
                }
            }
        }

        ContentManager manager;
    }
}