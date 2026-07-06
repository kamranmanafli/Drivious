using Drivious.Models.BaseModels;

namespace Drivious.Models
{
    public class FuelLog : BaseEntity
    {
        public Guid VehicleId { get; set; }          // Hansı maşına yanacaq vurulub

        public decimal Liters { get; set; }          // Neçə litr yanacaq vurulub

        public decimal Price { get; set; }           // Ümumi ödənilən məbləğ

        public DateTime FuelDate { get; set; }       // Yanacaq vurulan tarix

        public int Mileage { get; set; }             // Yanacaq vurulanda maşının kilometri

        public string StationName { get; set; }      // Hansı yanacaqdoldurma məntəqəsi

        public Vehicle Vehicle { get; set; }         // Əlaqəli maşın
    }
}
