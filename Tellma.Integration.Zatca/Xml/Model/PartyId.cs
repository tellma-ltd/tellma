namespace Tellma.Integration.Zatca
{
    /// <summary>
    /// Represents a party ID and scheme pair.
    /// </summary>
    public readonly struct PartyId
    {
        /// <summary>
        /// Create a an instance of <see cref="PartyId"/>.
        /// </summary>
        /// <param name="scheme">Other party ID scheme.</param>
        /// <param name="value">Other party ID.</param>
        public PartyId(PartyIdScheme scheme, string value)
        {
            Scheme = scheme;
            Value = value;
        }

        /// <summary>
        /// <b>BT-29-1</b>, <b>BT-46-1</b> 
        /// <br/> 
        /// Other party ID scheme.
        /// </summary>
        public PartyIdScheme Scheme { get;  }

        /// <summary>
        /// <b>BT-29</b>, <b>BT-46</b> 
        /// <br/> 
        /// Other party ID.
        /// </summary>
        public string Value { get; }
    }
}
