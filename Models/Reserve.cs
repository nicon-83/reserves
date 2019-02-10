//------------------------------------------------------------------------------
// Модель информации о резерве
//------------------------------------------------------------------------------

namespace Portal.Models
{
    using System;
    
    public partial class Reserve
    {
        public int idReserve { get; set; }//id резерва
        public string number { get; set; }//номер резерва
        public DateTime reserveDate { get; set; }//дата создания резерва
        public DateTime receiveDate { get; set; }//дата получения резерва
        public string source { get; set; }//источник
        public string idApteka { get; set; }//id аптеки
        public string nameApteka { get; set; }//наименование аптеки
        public string fio { get; set; }//имя клиента
        public string state { get; set; }//код статуса резерва
        public string description { get; set; } //описание статуса
        public string delReason { get; set; }//причина аннулирования (столбец состояние)
        public string comments { get; set; }
    }
}
