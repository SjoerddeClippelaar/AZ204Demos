// found on: https://docs.microsoft.com/en-us/archive/msdn-magazine/2018/january/data-points-creating-azure-functions-to-interact-with-cosmos-db


using System.Net;
public static async Task<HttpResponseMessage> Run(HttpRequestMessage req,
  TraceWriter log,    IAsyncCollector<object> outputDocument)
{
  BingeRequest bingeData =  await req.Content.ReadAsAsync<BingeRequest>();
  log.Verbose("Incoming userId:" + bingeData.userId);
  var doc=new BingeDocument(bingeData,log);
  log.Verbose("Outgoing userId:" + doc.userId);
  await outputDocument.AddAsync(doc);
  if (doc.userId !=" " ){
    return req.CreateResponse(HttpStatusCode.OK,$"{doc.userId} was created" );
  }
  else {
    return req.CreateResponse(HttpStatusCode.BadRequest,
      $"The request was incorrectly formatted." );
  }
}
public class BingeRequest{ . . . }
public class BingeDocument { . . . }