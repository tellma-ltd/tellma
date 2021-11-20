namespace Tellma.Api.Instances
{
    public class InstancesOptions
    {
        /// <summary>
        /// How often do instances signal their livelihood in the Admin DB.<br/>
        /// - Too often => the Admin DB will be overwhelmed.<br/>
        /// - Too rarely => orphan tenants may remain orphans for too long before adopted by another instance.<br/>
        /// - Happy medium => A frequency where 50 instances beating together are still far from overwhelming the admin database.<br/>
        /// </summary>
        public int InstanceHeartRateInSeconds { get; set; } = 60; // 1 minute

        /// <summary>
        /// How many orphans every instance adopts every <see cref="OrphanAdoptionFrequencyInSeconds"/>.<br/>
        /// - Too many => will cause an adoption imbalance; one instance adopts many more tenants than other instances.<br/>
        /// - Too few => Orphans may remain orphans for a while before adopted.<br/>
        /// - Happy medium => expected number of tenants / expected number of instances (ballpark figure).<br/>
        /// </summary>
        public int OrphanAdoptionBatchCount { get; set; } = 20;

        /// <summary>
        /// Autocomputed.
        /// </summary>
        public int OrphanAdoptionFrequencyInSeconds => InstanceHeartRateInSeconds * 5;

        /// <summary>
        /// Autocomputed.
        /// </summary>
        public int InstanceKeepAliveInSeconds => InstanceHeartRateInSeconds * 10;
    }
}
