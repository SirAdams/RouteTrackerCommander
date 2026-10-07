using System;using System.Collections.Generic;using System.Web.Script.Serialization;using RouteTrackerCommander;
class UpdateTests {
 static int checks;static void A(bool b,string name){checks++;if(!b)throw new Exception(name);}
 static Dictionary<string,object> Release(string tag,string host="EDD19.1.11",bool draft=false,bool preview=false,string url=null){string asset="RouteTrackerCommander-"+tag.TrimStart('v')+"-"+host+".zip";return new Dictionary<string,object>{{"tag_name",tag},{"draft",draft},{"prerelease",preview},{"assets",new object[]{new Dictionary<string,object>{{"name",asset},{"browser_download_url",url??ReleaseChecker.Repository+"/releases/download/"+tag+"/"+asset}}}}};}
 static ReleaseUpdate Select(params object[] releases){return ReleaseChecker.SelectRelease(new JavaScriptSerializer().Serialize(releases),new Version(1,1,2,0),"EDD19.1.11");}
 static int Main(){try{
 A(Select()==null,"Empty release list");
 A(Select(Release("v1.1.1"))==null,"Older ignored");
 A(Select(Release("v1.1.2"))==null,"Equal 3-part version ignored");
 A(Select(Release("v1.1.2.0"))==null,"Equal 4-part version ignored");
 A(Select(Release("v1.1.3")).Version==new Version(1,1,3,0),"Newer selected");
 A(Select(Release("v1.1.3","EDD20.x"))==null,"Wrong host ignored");
 A(Select(Release("v1.1.3","EDD19.1.11",true))==null,"Draft ignored");
 A(Select(Release("v1.1.3","EDD19.1.11",false,true)).Preview,"Preview supported and labelled");
 A(Select(Release("v1.1.3"),Release("v1.1.9"),Release("v1.1.4")).Version==new Version(1,1,9,0),"Highest compatible release selected");
 A(Select(Release("v1.1.3"),Release("v2.0.0","EDD20.x")).Version==new Version(1,1,3,0),"Newer wrong-host release does not hide compatible one");
 A(Select(Release("v1.1.3","EDD19.1.11",false,false,"https://example.com/download.zip"))==null,"External download rejected");
 A(Select(Release("v1.1.3","EDD19.1.11",false,false,"https://github.com/other/repo/releases/download/v1.1.3/x.zip"))==null,"Wrong repository rejected");
 A(Select(Release("v1.1.3","EDD19.1.11",false,false,"http://github.com/SirAdams/RouteTrackerCommander/releases/download/v1.1.3/x.zip"))==null,"HTTP rejected");
 A(Select(Release("v1.1.3-beta"))==null,"Unsupported tag ignored");
 A(Select(Release("../../bad"))==null,"Unsafe tag ignored");
 var missing=Release("v1.1.3");missing.Remove("assets");A(Select(missing)==null,"Missing assets ignored");
 var wrong=Release("v1.1.3");wrong["assets"]=new object[]{new Dictionary<string,object>{{"name","unrelated.zip"},{"browser_download_url",""}}};A(Select(wrong)==null,"Unrelated ZIP ignored");
 A(Select(Release("v1.1.3")).ReleaseUrl==ReleaseChecker.Repository+"/releases/tag/v1.1.3","Release URL remains in trusted repository");
 bool invalid=false;try{ReleaseChecker.SelectRelease("{}",new Version(1,1,2,0),"EDD19.1.11");}catch(FormatException){invalid=true;}A(invalid,"Invalid response fails safely");
 Console.WriteLine("PASS "+checks+" update selection assertions (offline)");return 0;
 }catch(Exception e){Console.WriteLine(e);return 1;}}
}
