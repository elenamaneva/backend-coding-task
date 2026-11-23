using System.Text.Json.Serialization;
using MongoDB.Bson.Serialization.Attributes;

namespace Claims
{
    /// <summary>
    /// Represents an insurance claim for damage to a covered vessel.
    /// </summary>
    public class Claim
    {
        /// <summary>
        /// Unique identifier for the claim.
        /// </summary>
        [BsonId]
        public string Id { get; set; }

        /// <summary>
        /// The ID of the related Cover (insurance policy).
        /// </summary>
        [BsonElement("coverId")]
        public string CoverId { get; set; }

        /// <summary>
        /// Date when the claim was created. Must be within the Cover period.
        /// </summary>
        [BsonElement("created")]
        public DateTime Created { get; set; }

        /// <summary>
        /// Description of the claim.
        /// </summary>
        [BsonElement("name")]
        public string Name { get; set; }

        /// <summary>
        /// Type of claim. Available values: Collision, Grounding, BadWeather, Fire
        /// </summary>
        [BsonElement("claimType")]
        public ClaimType Type { get; set; }

        /// <summary>
        /// Cost of the damage. Cannot exceed 100,000.
        /// </summary>
        [BsonElement("damageCost")]
        public decimal DamageCost { get; set; }
    }

    /// <summary>
    /// Types of insurance claims.
    /// </summary>
    public enum ClaimType
    {
        /// <summary>Ship crashed into another vessel, dock, or object</summary>
        Collision = 0,
        
        /// <summary>Ship ran aground (hit sea floor or beach)</summary>
        Grounding = 1,
        
        /// <summary>Damage caused by storms, waves, or wind</summary>
        BadWeather = 2,
        
        /// <summary>Fire broke out on the vessel</summary>
        Fire = 3
    }
}
