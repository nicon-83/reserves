//------------------------------------------------------------------------------
// Модель информации о состоянии резерва
//------------------------------------------------------------------------------

namespace Portal.Models
{
    using System;

    public partial class ReserveState
    {
        public string number { get; set; }//номер резерва
        public DateTime? stateDate { get; set; }//дата присвоения статуса
        public string state { get; set; }//код статуса
        public string description { get; set; }//описание кода статуса
    }
}
