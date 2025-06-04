using System.Data.SqlTypes;
using WedMockExam.Repository.Helpers;

namespace WedMockExam.Repository.Interfaces.PreferredLocation
{
    public class PreferredLocationFilter
    {
        public int? UserId { get; set; }
        public int? WorkplaceId { get; set; }
        public int? PreferenceRank { get; set; }
    }
}
    