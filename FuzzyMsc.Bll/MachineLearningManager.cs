using FuzzyMsc.Bll.Interface;
using FuzzyMsc.Dto;
using FuzzyMsc.Dto.CizimDTOS;
using FuzzyMsc.Pattern.UnitOfWork;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Text;

namespace FuzzyMsc.Bll
{
	public class MachineLearningManager : IMachineLearningManager
	{
		IUnitOfWorkAsync _unitOfWork;
		IOrtakManager _ortakManager;

		public MachineLearningManager(IUnitOfWorkAsync unitOfWork,
			IOrtakManager ortakManager)
		{
			_unitOfWork = unitOfWork;
			_ortakManager = ortakManager;
		}

		public SonucDTO Test(MachineLearningDTO datas)
		{
			SonucDTO sonuc = new SonucDTO();
			var postResult = POST("http://localhost:5555/postjson", datas);

			return sonuc;
		}

		private SonucDTO POST(string url, MachineLearningDTO datas)
		{
			string resultStream = "";
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
			request.Method = "POST";

			System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
			datas.excel.data = null;
			var jsonContent =  JsonConvert.SerializeObject(new MachineLearningJsonDTO { path = datas.excel.path, algorithm = datas.algorithm});
			Byte[] byteArray = encoding.GetBytes(jsonContent);

			request.ContentLength = byteArray.Length;
			request.ContentType = "application/json";

			using (Stream dataStream = request.GetRequestStream())
			{
				dataStream.Write(byteArray, 0, byteArray.Length);
			}
			long length = 0;
			try
			{
				using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
				{
					// got response
					
					length = response.ContentLength;

					using (Stream responseStream = response.GetResponseStream())
					{
						StreamReader reader = new StreamReader(responseStream, Encoding.GetEncoding("utf-8"));
						resultStream = reader.ReadToEnd();
						return JsonConvert.DeserializeObject<SonucDTO>(resultStream);
					}
				}
			}
			catch (WebException ex)
			{
				WebResponse errorResponse = ex.Response;
				using (Stream responseStream = errorResponse.GetResponseStream())
				{
					StreamReader reader = new StreamReader(responseStream, Encoding.GetEncoding("utf-8"));
					resultStream = reader.ReadToEnd();
					return JsonConvert.DeserializeObject<SonucDTO>(resultStream);
				}
				throw;
			}
		}

		public SonucDTO GET(string url)
		{
			SonucDTO model = new SonucDTO();
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
			try
			{
				WebResponse response = request.GetResponse();
				using (Stream responseStream = response.GetResponseStream())
				{
					StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
					model = JsonConvert.DeserializeObject<SonucDTO>(reader.ReadToEnd());
				}
			}
			catch (WebException ex)
			{
				WebResponse errorResponse = ex.Response;
				using (Stream responseStream = errorResponse.GetResponseStream())
				{
					StreamReader reader = new StreamReader(responseStream, Encoding.GetEncoding("utf-8"));
					String errorText = reader.ReadToEnd();
					// log errorText
				}
				throw;
			}

			return model;
		}
	}

	public interface IMachineLearningManager : IBaseManager
	{
		SonucDTO Test(MachineLearningDTO datas);
	}
}
