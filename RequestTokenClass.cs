using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaxCollectData.Library.Dto;
using TaxCollectData.Library.Dto.Config;
using TaxCollectData.Library.Business;
using TaxCollectData.Library.Dto.Content;
using TaxCollectData.Library.Dto.Properties;
using TaxCollectData.Library.Enums;
using System.Windows.Forms;

namespace TaxMoadian
{
    public class RequestTokenClass
    {
        //*****
        //در هنگام فراخوانی این کلاس مقادیر مورد نظ پایین را ارسال میکنیم سپس در سطر بعد
        //تابع انتیتی را فراخوانی کرده و توکن را درخروجی دریافت میکنیم


        public string PrivateKey = "کلید خصوص شما";
        public string ShenaseYkta = "شناسه یکتای شما";
        public string ServiceUrl = "https://tp.tax.gov.ir/req/api/";
        public RequestTokenClass(string PKey,string ShYekta)
        {
            PrivateKey = PKey;
            ShenaseYkta = ShYekta;
        }

        public TokenModel Entity()
        {
            TokenModel Token=null;
            try
            {
                SignatoryConfig signatory = new SignatoryConfig(PrivateKey, null);// ساخت امضا
                TaxApiService.Instance.Init(ShenaseYkta, signatory, new NormalProperties(ClientType.SELF_TSP), ServiceUrl);
                ServerInformationModel serverInformation = TaxApiService.Instance.TaxApis.GetServerInformation();// گرفتن اطلاعات سرور
                Token = TaxApiService.Instance.TaxApis.RequestToken();//  درخواست توکن
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return Token;
        }
    }


}
