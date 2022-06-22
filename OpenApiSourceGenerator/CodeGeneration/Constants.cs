namespace NatMarchand.OpenApiSourceGenerator.CodeGeneration;

internal static class Constants
{
    public static class Namespace
    {
        public const string SystemComponentmodelDataannotations = "System.ComponentModel.DataAnnotations";
        public const string SystemThreading = "System.Threading";
        public const string SystemRuntimeCompilerservices = "System.Runtime.CompilerServices";
        public const string MicrosoftAspnetcoreMvc = "Microsoft.AspNetCore.Mvc";
        public const string MicrosoftOpenapiModels = "Microsoft.OpenApi.Models";
    }

    public static class Attribute
    {
        public const string Obsolete = "Obsolete";
        public const string CompilerGenerated = "CompilerGenerated";

        public const string Required = "Required";

        public const string ApiController = "ApiController";
        public const string ApiExplorerSettings = "ApiExplorerSettings";

        public const string FromHeader = "FromHeader";
        public const string FromBody = "FromBody";
        public const string FromQuery = "FromQuery";
        public const string FromRoute = "FromRoute";
        
        public const string ProducesDefaultResponseType = "ProducesDefaultResponseType";
        public const string ProducesResponseType = "ProducesResponseType";
    }

    public static class Type
    {
        public const string Boolean = "bool";
        public const string Long = "long";
        public const string Int = "int";
        public const string Float = "float";
        public const string Double = "double";
        public const string String = "string";
        public const string DateTime = "DateTime";
        public const string DateOnly = "DateOnly";
        public const string Guid = "Guid";
        public const string ByteArray = "byte[]";

        public const string CancellationToken = "CancellationToken";
        public const string Task = "Task";


        public const string ProblemDetails = "ProblemDetails";
        public const string ControllerBase = "ControllerBase";
        public const string IActionResult = "IActionResult";
        public const string ActionResult = "ActionResult";

        public const string OpenApiInfo = "OpenApiInfo";
        
    }

    public static class XmlDoc
    {
        public const string SummarySection = "summary";
        public const string RemarksSection = "remarks";
        public const string ResponseSection = "response";
        public const string ParamSection = "param";
        public const string ParamNameAttribute = "name";
        public const string ResponseCodeAttribute = "code";
    }
}