using Microsoft.AspNetCore.Mvc;
using Portal.Models;
using PortalCore.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace PortalCore.Controllers
{
    public class ADOController : Controller
    {
        private readonly string connectionString = new SqlConnectionStringBuilder
        {
            DataSource = "",
            InitialCatalog = "",
            UserID = "",
            Password = "",
            Pooling = true,
        }.ConnectionString;
        private readonly CultureInfo MyCultureInfo = new CultureInfo("ru-RU");

        //начальная страница
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        //поиск резервов по номеру или за указанный период (Ajax)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> _IndexAjaxAsync()
        {
            string Date1 = DateTime.Now.ToString("dd.MM.yyyy");
            string Date2 = DateTime.Now.ToString("dd.MM.yyyy");
            string number = string.Empty;//номер резерва, который указываем в поиске на странице
            List<Reserve> data = new List<Reserve>();
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    if (Request.Method == "POST")
                    {
                        if (!string.IsNullOrEmpty(Request.Form["Text"]))
                        {
                            number = Request.Form["Text"];
                            number = number.Replace(" ", "");
                        }

                        Date1 = Request.Form["Date1"];
                        //код для корректной обработки даты браузером InternetExplorer
                        byte[] asciiBytes1 = Encoding.ASCII.GetBytes(Date1);
                        string d1 = Encoding.ASCII.GetString(asciiBytes1);
                        d1 = d1.Replace("?", "");

                        Date2 = Request.Form["Date2"];
                        byte[] asciiBytes2 = Encoding.ASCII.GetBytes(Date2);
                        string d2 = Encoding.ASCII.GetString(asciiBytes2);
                        d2 = d2.Replace("?", "");

                        Date1 = d1;
                        Date2 = d2;

                        if (DateTime.Parse(Date1, MyCultureInfo) > DateTime.Parse(Date2, MyCultureInfo))
                        {
                            ViewBag.Message = "Внимание! Дата начала периода должна быть меньше или равна дате окончания периода";
                            return View("MessageView");
                        }
                        SqlCommand query = new SqlCommand
                        {
                            CommandText = $@"select rr.idReserve,
                                                       rr.reserveDate,
                                                       rr.receiveDate,
                                                       ISNULL(rr.number, '')                     number,
                                                       ISNULL(rr.source, '')                     source,
                                                       CAST(rr.idApteka AS VARCHAR(50))                    idApteka,
                                                       ISNULL(cl.naimen, '')                     nameApteka,
                                                       ISNULL(rr.fio, '')                        fio,
                                                       ISNULL(CAST(rr.state AS VARCHAR(50)), '') state,
                                                       CASE rr.state
		                                                   WHEN -1 THEN 'Отменен (разобрать)'
		                                                   WHEN 1 THEN 'Новый'
		                                                   WHEN 2 THEN 'Готов к выдаче'
		                                                   WHEN 4 THEN 'Ожидает подтверждения покупателя'
		                                                   WHEN 5 THEN 'Подтвержден покупателем'
		                                                   WHEN 6 THEN 'Отменен'
		                                                   WHEN 8 THEN 'Выдан'
		                                                   WHEN 9 THEN 'Комплектуется'
                                                           WHEN 20 THEN 'Дозаказ'
		                                                   ELSE 'Нет описания статуса'
	                                                   END AS description,
                                                       ISNULL(rr.delReason, '')                            delReason,
                                                       ISNULL(rr.comments, '')                             comments
                                                from (select r.idReserve, r.reserveDate, r.receiveDate, r.number, r.source, r.idApteka, r.fio, r.state, r.delReason, r.comments
                                                      from Reserve r
                                                      WHERE CAST(r.receiveDate AS DATE) BETWEEN @Date1 and @Date2
                                                        and (r.number like @number or r.idApteka = case
                                                                                                       when isnumeric(@idApteka) = 1
                                                                                                               then cast(@idApteka as int)
                                                                                                       else 0 end)) rr
                                                       left join admzakaz.dbo.clients cl on rr.idApteka = cl.kp
                                                ORDER BY rr.receiveDate DESC",
                            Connection = connection
                        };
                        query.Parameters.AddWithValue("Date1", Date1);
                        query.Parameters.AddWithValue("Date2", Date2);
                        query.Parameters.AddWithValue("number", "%" + number + "%");
                        query.Parameters.AddWithValue("idApteka", number);
                        SqlDataReader reader = await query.ExecuteReaderAsync();
                        if (reader.HasRows)
                        {
                            while (await reader.ReadAsync())
                            {
                                Reserve row = new Reserve
                                {
                                    idReserve = reader.GetInt32(0),
                                    reserveDate = reader.GetDateTime(1),
                                    receiveDate = reader.GetDateTime(2),
                                    number = reader["number"] as string,
                                    source = reader["source"] as string,
                                    idApteka = reader["idApteka"] as string,
                                    nameApteka = reader["nameApteka"] as string,
                                    fio = reader["fio"] as string,
                                    state = reader["state"] as string,
                                    description = reader["description"] as string,
                                    delReason = reader["delReason"] as string,
                                    comments = reader["comments"] as string,
                                };
                                data.Add(row);
                            }
                        }
                        else
                        {
                            if (number == "")
                            {
                                ViewBag.Message = $"За период с {Date1} по {Date2} не найдено резервов";
                            }
                            else
                            {
                                ViewBag.Message = $"За период с {Date1} по {Date2} не найден резерв с номером {number}";
                            }
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }
            catch (Exception e)
            {
                ViewBag.Message = "Ошибка: " + e.Message + Environment.NewLine + e.StackTrace;
            }
            return PartialView("_IndexAjax", data);
        }

        //метод для вывода состава резерва в выпадающем окне (Ajax)
        [HttpPost]
        public async Task<IActionResult> _DetailInDropDownAsync()
        {
            List<ReserveGoods> data = new List<ReserveGoods>();

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    if (!string.IsNullOrEmpty(Request.Form["idReserve"]))
                    {
                        string idReserve = Request.Form["idReserve"];
                        idReserve = idReserve.Replace(" ", "");

                        await connection.OpenAsync();

                        SqlCommand query = new SqlCommand
                        {
                            CommandText = $@"SELECT 
                                               ISNULL(r.number, '')                                                number,
                                               ISNULL(CAST(r.idpos AS VARCHAR(50)), '')                            idpos,
                                               ISNULL(np.shortname, '')                                            shortname,
                                               ISNULL(CAST(r.idpro AS VARCHAR(50)), '')                            idpro,
                                               case
                                                when r.idpro = 0 then isnull(pr.name, '') + isnull(pr.NAME_COUNT, '')
                                                when r.idpro is null then isnull(pr.name, '') + isnull(pr.NAME_COUNT, '')
                                                else isnull(pr.name, '') + ' (' + isnull(pr.NAME_COUNT, '') + ')'
                                                   end as                                                          name,
                                               isnull(CONVERT(VARCHAR(50), CONVERT(float, r.cost)), '0')           cost,
                                               isnull(CONVERT(VARCHAR(50), CONVERT(float, r.Amount)), '0')         amount,
                                               isnull(r.idposList, '')                                             idposList,
                                               isnull(CONVERT(VARCHAR(50), CONVERT(float, r.amounta)), '0')        amounta,
                                               isnull(CONVERT(VARCHAR(50), CONVERT(float, r.amountAnnul)), '0')    amountannul,
                                               isnull(CONVERT(VARCHAR(50), CONVERT(float, r.amountComplete)), '0') amountcomplete,
                                               isnull(CONVERT(VARCHAR(50), CONVERT(float, r.amountOrdered)), '0')  amountordered,
                                               isnull(CONVERT(VARCHAR(50), CONVERT(float, r.amountPacked)), '0')   amountpacked,
                                               isnull(CONVERT(VARCHAR(50), CONVERT(float, r.Amount1)), '0')        amount1,
                                               isnull(CONVERT(VARCHAR(50), CONVERT(float, r.Amount2)), '0')        amount2,
                                               isnull(CONVERT(VARCHAR(50), CONVERT(float, r.Amount4)), '0')        amount4,
                                               isnull(CONVERT(VARCHAR(50), CONVERT(float, r.Amount5)), '0')        amount5,
                                               isnull(CONVERT(VARCHAR(50), CONVERT(float, r.Amount6)), '0')        amount6,
                                               isnull(CONVERT(VARCHAR(50), CONVERT(float, r.Amount8)), '0')        amount8,
                                               isnull(CONVERT(VARCHAR(50), CONVERT(float, r.Amount9)), '0')        amount9,
                                               isnull(CONVERT(VARCHAR(50), CONVERT(float, r.Amount20)), '0')       amount20
                                            FROM vReserveGoods r
                                                    LEFT JOIN qwerty_base.dbo.npos np ON r.idpos = np.idpos
                                                    LEFT JOIN qwerty_base.dbo.PRO pr ON r.idpro = pr.idpro
                                                WHERE idReserve = @idReserve
                                                ORDER BY r.idReserve DESC",
                            Connection = connection
                        };
                        query.Parameters.AddWithValue("idReserve", idReserve);
                        SqlDataReader reader = await query.ExecuteReaderAsync();
                        if (reader.HasRows)
                        {
                            while (await reader.ReadAsync())
                            {
                                ViewBag.ReserveNumber = reader["number"] as string;

                                ReserveGoods row = new ReserveGoods
                                {
                                    Number = reader["number"] as string,
                                    Idpos = reader["idpos"] as string,
                                    Shortname = reader["shortname"] as string,
                                    Idpro = reader["idpro"] as string,
                                    Name = reader["name"] as string,
                                    Cost = reader["cost"] as string,
                                    Amount = reader["amount"] as string,
                                    Amounta = reader["amounta"] as string,
                                    AmountAnnul = reader["amountannul"] as string,
                                    AmountComplete = reader["amountcomplete"] as string,
                                    AmountOrdered = reader["amountordered"] as string,
                                    AmountPacked = reader["amountpacked"] as string,
                                    Amount1 = reader["amount1"] as string,
                                    Amount2 = reader["amount2"] as string,
                                    Amount4 = reader["amount4"] as string,
                                    Amount5 = reader["amount5"] as string,
                                    Amount6 = reader["amount6"] as string,
                                    Amount8 = reader["amount8"] as string,
                                    Amount9 = reader["amount9"] as string,
                                    Amount20 = reader["amount20"] as string,
                                    IdposList = reader["idposList"] as string
                                };
                                data.Add(row);
                            }
                        }
                        else
                        {
                            ViewBag.Message = $"Резерв номером idReserve = {idReserve} не найден";
                        }
                        reader.Close();

                        connection.Close();
                    }
                    else
                    {
                        throw new Exception("Ошибка: не передан параметр idReserve!");
                    }
                }
            }
            catch (Exception e)
            {
                ViewBag.Message = "Ошибка: " + e.Message + Environment.NewLine + e.StackTrace;
            }

            return PartialView("_DetailInDropDown", data);
        }

        //метод для вывода истории статусов резерва в модальном окне (Ajax)
        public async Task<IActionResult> _HistoryInModalAsync(string id)
        {
            List<ReserveState> data = new List<ReserveState>();

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand query = new SqlCommand
                    {
                        CommandText = $@"SELECT ISNULL(r.number, '') number,
                                               rs.stateDate stateDate,
	                                           ISNULL(CAST(rs.state AS VARCHAR(20)), '') state,
	                                           CASE rs.state
		                                           WHEN -1 THEN 'Отменен (разобрать)'
		                                           WHEN 1 THEN 'Новый'
		                                           WHEN 2 THEN 'Готов к выдаче'
		                                           WHEN 4 THEN 'Ожидает подтверждения покупателя'
		                                           WHEN 5 THEN 'Подтвержден покупателем'
		                                           WHEN 6 THEN 'Отменен'
		                                           WHEN 8 THEN 'Выдан'
		                                           WHEN 9 THEN 'Комплектуется'
                                                   WHEN 20 THEN 'Дозаказ'
		                                           ELSE 'Нет описания статуса'
	                                           END AS description
                                        FROM (
	                                        SELECT number,
		                                           idReserve
	                                        FROM Reserve
	                                        WHERE idReserve = @id
                                        ) r
                                        LEFT JOIN ReserveState rs ON r.idReserve = rs.idReserve
                                        ORDER BY rs.stateDate",
                        Connection = connection
                    };
                    query.Parameters.AddWithValue("id", id);
                    SqlDataReader reader = await query.ExecuteReaderAsync();
                    if (reader.HasRows)
                    {
                        while (await reader.ReadAsync())
                        {
                            if (string.IsNullOrEmpty(ViewBag.ReserveNumber))
                            {
                                ViewBag.ReserveNumber = reader["number"] as string;
                            }

                            ReserveState row = new ReserveState
                            {
                                number = reader["number"] as string,
                                stateDate = (DateTime?)reader.GetDateTime(1) ?? default(DateTime),
                                state = reader["state"] as string,
                                description = reader["description"] as string
                            };
                            data.Add(row);
                        }
                    }
                    reader.Close();

                    connection.Close();
                }
            }
            catch (Exception e)
            {
                ViewBag.Message = "Ошибка: " + e.Message + Environment.NewLine + e.StackTrace;
            }

            return PartialView("_HistoryInModal", data);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}