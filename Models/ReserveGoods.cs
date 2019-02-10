//------------------------------------------------------------------------------
// Модель состава резерва
//------------------------------------------------------------------------------

namespace Portal.Models
{

    public partial class ReserveGoods
    {
        public string Number { get; set; }//номер резерва
        public string Idpos { get; set; }//id товара
        public string Shortname { get; set; }//наименование товара
        public string Idpro { get; set; }//id производителя товара
        public string Name { get; set; }//наименование производителя товара
        public string Cost { get; set; }//стоимость товара
        public string Amount { get; set; }//количество
        public string IdposList { get; set; }//список подмен
        public string Amounta { get; set; }//Зарезервировано
        public string AmountAnnul { get; set; }//Аннулировано
        public string AmountOrdered { get; set; }//Заказано
        public string AmountPacked { get; set; }//Укомплектовано
        public string AmountComplete { get; set; }//Выдано
        public string Amount1 { get; set; }//Новый
        public string Amount2 { get; set; }//Готов к выдаче
        public string Amount4 { get; set; }//Ожидает подтверждения покупателя
        public string Amount5 { get; set; }//Подтвержден покупателем
        public string Amount6 { get; set; }//Отменен
        public string Amount8 { get; set; }//Выдан
        public string Amount9 { get; set; }//Комплектуется
        public string Amount20 { get; set; }//Дозаказ
    }
}
