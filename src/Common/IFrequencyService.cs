using System;

namespace Microsoft.WindowsAzure.Commands.Common
{
    /// <summary>
    /// Interface for a service that manages the frequency of business logic execution based on configured feature flags.
    /// </summary>
    public interface IFrequencyService
    {
        /// <summary>
        /// Checks if the specified feature is enabled and if it's time to run the business logic based on the feature's frequency. 
        /// If both conditions are met, it runs the specified business action.
        /// </summary>
        /// <param name="featureName">The name of the feature to check.</param>
        /// <param name="businessCheck">A function that returns true if the business logic should be executed.</param>
        /// <param name="business">An action to execute if the business logic should be executed.</param>
        void TryRun(string featureName, Func<bool> businessCheck, Action business);

        /// <summary>
        /// Registers a feature with the specified name and frequency to the service.
        /// </summary>
        /// <param name="featureName">The name of the feature to add.</param>
        /// <param name="frequency">The frequency at which the business logic should be executed for the feature.</param>
        void Register(string featureName, TimeSpan frequency);

        /// <summary>
        /// Registers the specified feature to the service's per-PSsession registry.
        /// </summary>
        /// <param name="featureName">The name of the feature to add.</param>
        void RegisterInSession(string featureName);

        /// <summary>
        /// Saves the current state of the service to persistent storage.
        /// </summary>
        void Save();
    }
}
