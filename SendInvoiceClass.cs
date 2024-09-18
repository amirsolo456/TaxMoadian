using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TaxCollectData.Library.Business;
using TaxCollectData.Library.Dto.Content;

namespace TaxMoadian
{
    public class SendInvoiceClass
    {
        private string PrivateKey = "کلید خصوصی شما";
        private string PublicKey = "کلید عمومی شما";
        private string ShenaseYekta = " شناسه یکتای شما";

        public SendInvoiceClass()
        {

        }


        //کلاس جهت ریخت پاسخ ارسال داخلش
        public class SendResponse()
        {
            public string IrTaxID { get; set; }
            public string Packettype { get; set; }
            public string Status { get; set; }
            public string UID { get; set; }
            public string RefNumber { get; set; }
            public string ErrorMsg { get; set; }
        }

        public SendResponse SendInvoice()
        {
            SendResponse response = new SendResponse();
            try
            {
                Int64 SerialNumber = 1100000; //تعریف سریال نامبر در داکیومنت های سامانه می باشد  اما خلاصه یک عدد از جنس اینت 64 است که رندوم تولید باید بشود
                string TaxId = GetTaxID(SerialNumber, ShenaseYekta);

                var Header = new InvoiceHeaderDto()
                {
                    Taxid = TaxId,       //شماره منحصر بفرد مالیاتی
                    Indatim = 23134,      //تاریخ و زمان صدور صورتحساب)میلادی(
                    Inty = 1,            //نوع صورتحساب
                    Inno = "",            //سریال صورتحساب
                    Irtaxid = "",        //شماره منحصر بفرد مالیاتی مرجع
                    Inp = 2,             //الگوی صورتحساب
                    Ins = 1,             //وضوع صورتحساب شامل: اصلی ،1 اصالحی ،2 ابطالی 3 و برگشت از فروش ،4 است                    
                    Tins = "",            //شماره اقتصادی فروشنده
                    Tob = 1,             //نوع شخص باید مطابق اطالعات ثبت نام الکترونیک و شامل :حقیقی ،1 حقوقی،2 مشارکت مدنی ،3 اتباع غیر ایرانی ،4 باشد.
                    Bid = "",             //شماره/شناسه ملی/شناسه مشارکت مدنی/کد فراگیر خریدار
                    Tinb = "",           //شماره اقتصادی خریدار
                    Crn = "",            //شناسه یکتای ثبت قرارداد فروشنده
                    Billid = "",         //شماره اشتراک/ شناسه قبض بهره بردار
                    Tprdis = 14000,      //مجموع مبلغ قبل از کسر تخفیف 
                    Tdis = 0,            //مجموع تخفیفات
                    Tadis = 115000,      //مجموع مبلغ پس از کسر تخفیف 
                    Sbc = "",            // کد شعبه فروشنده
                    Bbc = "",             // کد شعبه خریدار
                    Tvam = 12000,        //مجموع مالیات بر ارزش افزوده
                    Todam = 0,           //مجموع سایر مالیات، عوارض و وجوه قانونی
                    Tbill = 250000,      //مجموع صورتحساب
                    Setm = 1,            //روش تسویه
                    Cap = 0,              //مبلغ پرداخت نقدی
                    Insp = 1250000,      //مبلغ پرداختی نسیه
                    Tvop = 0,            //مجموع سهم مالیات بر ارزش افزوده از پرداخت 

                };

                var BodyList = new List<InvoiceBodyDto>() { };

                PaymentDto paymentDto = new PaymentDto()
                {
                };


                var invoices = new List<InvoiceDto>() {
                   new InvoiceDto {

                     Body  =  BodyList,
                     Header = Header,
                     Payments = new() { paymentDto }

                  }
                };

                var responseModel = TaxApiService.Instance.TaxApis.SendInvoices(invoices, null);//ارسال صورتحساب
                if (responseModel != null)
                {
                    if (responseModel.Body != null)
                    {
                        Thread.Sleep(3000);

                        var Response = responseModel.Body.Result.First();
                        var Estelam = TaxApiService.Instance.TaxApis.InquiryByReferenceId(new() { Response.ReferenceNumber });

                        if (Estelam[0].Status == "NOTFOUND")
                        {
                            Thread.Sleep(3000);
                            Estelam = TaxApiService.Instance.TaxApis.InquiryByReferenceId(new() { Response.ReferenceNumber });
                            if (Estelam[0].Status == "NOTFOUND")
                            {
                                var uidAndFiscalId = new UidAndFiscalId(Response.Uid, ShenaseYekta);
                                Estelam = TaxApiService.Instance.TaxApis.InquiryByUidAndFiscalId(new() { uidAndFiscalId });
                                if (Estelam[0].Status == "NOTFOUND")
                                {
                                    return null;
                                }
                            }
                        }

                        response.IrTaxID = TaxId;
                        response.Packettype = Estelam[0].PacketType;
                        response.Status = Estelam[0].Status;
                        response.UID = Response.Uid;
                        response.RefNumber = Response.ReferenceNumber;
                        string ErrMsg = "";
                        ErrMsg = (Estelam[0].Data != null ? Estelam[0].Data.ToString() : "");
                        if (ErrMsg.Length > 1990) ErrMsg = ErrMsg.Substring(0, 1990);
                        response.ErrorMsg = ErrMsg;

                        return response;
                    }
                    else
                    {
                        MessageBox.Show("سرور دارایی پاسخگو نیست!");
                    }
                }
                else
                {
                    MessageBox.Show("خطایی در ارسال رخ داده است!");
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
            return response;
        }

        private string GetTaxID(long serial, string SanadSabtDate, bool Ebtal = false)
        {
            string taxId = "";
            try
            {
                int hour = 0;
                if (Ebtal == true) hour = 1;
                ////گرفتن تایم

                PersianCalendar persianCalendar = new PersianCalendar();
                string[] dateComponents = SanadSabtDate.Split('/');
                int year = int.Parse(dateComponents[0]);
                int month = int.Parse(dateComponents[1]);
                int day = int.Parse(dateComponents[2]);
                DateTime gregorianDate = persianCalendar.ToDateTime(year, month, day, hour, 0, 0, 0);
                //var SodurDate = new DateTimeOffset(gregorianDate).ToUnixTimeMilliseconds();
                //=============================


                //ساخت تکس آی دی
                taxId = TaxApiService.Instance.TaxIdGenerator.GenerateTaxId(ShenaseYekta, serial,
               gregorianDate);
                //=============================

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return taxId;
        }
    }
}
