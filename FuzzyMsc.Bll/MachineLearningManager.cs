using FuzzyMsc.Bll.Interface;
using FuzzyMsc.Dto;
using FuzzyMsc.Dto.MachineLearningDTOS;
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
		ICommonManager _commonManager;

		public MachineLearningManager(IUnitOfWorkAsync unitOfWork,
			ICommonManager commonManager)
		{
			_unitOfWork = unitOfWork;
			_commonManager = commonManager;
		}

		public ResultDTO Test(MachineLearningDTO datas)
		{
			ResultDTO sonuc = new ResultDTO();
			var postResult = POST("http://localhost:5555/calculateaccuracy", datas);

			return postResult;
		}

		private ResultDTO POST(string url, MachineLearningDTO datas)
		{
			string resultStream = "";
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
			request.Method = "POST";

			System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
			var jsonContent =  JsonConvert.SerializeObject(datas);
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
						return JsonConvert.DeserializeObject<ResultDTO>(resultStream);
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
					return JsonConvert.DeserializeObject<ResultDTO>(resultStream);
				}
				throw;
			}
		}

		public ResultDTO GET(string url)
		{
			ResultDTO model = new ResultDTO();
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
			try
			{
				WebResponse response = request.GetResponse();
				using (Stream responseStream = response.GetResponseStream())
				{
					StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
					model = JsonConvert.DeserializeObject<ResultDTO>(reader.ReadToEnd());
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

		public ResultDTO CreateAndSaveModel()
		{
			ResultDTO sonuc = new ResultDTO();
			var postResult = POST("http://localhost:5555/createandsavemodel", null);

			return postResult;
		}
	}

	public interface IMachineLearningManager : IBaseManager
	{
		ResultDTO CreateAndSaveModel();
		ResultDTO Test(MachineLearningDTO datas);
	}
}
