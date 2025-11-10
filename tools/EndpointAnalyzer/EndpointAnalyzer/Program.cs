using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

var repoRoot = args.Length > 0
    ? Path.GetFullPath(args[0])
    : Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "..", "..", "..", ".."));

var outputPath = Path.Combine(repoRoot, "analysis", "endpoint-analysis.json");

try
{
    var debugDir = Path.Combine(AppContext.BaseDirectory, "debug");
    Directory.CreateDirectory(debugDir);
    File.AppendAllText(Path.Combine(debugDir, "endpoint_analyzer_debug.txt"),
        $"[{DateTime.UtcNow:O}] repoRoot={repoRoot}, outputPath={outputPath ?? "(none)"}{Environment.NewLine}");
}
catch
{
    // ignore
}

var webApiControllerDir = Path.Combine(repoRoot, "src", "WebApi", "Controllers");
var portalRoot = Path.Combine(repoRoot, "src", "MerchantPortal");

if (!Directory.Exists(webApiControllerDir))
{
    Console.Error.WriteLine($"WebApi controller klasörü bulunamadı: {webApiControllerDir}");
    return 1;
}

if (!Directory.Exists(portalRoot))
{
    Console.Error.WriteLine($"MerchantPortal klasörü bulunamadı: {portalRoot}");
    return 1;
}

var webApiEndpoints = ExtractWebApiEndpoints(webApiControllerDir)
    .OrderBy(e => e.Controller)
    .ThenBy(e => e.HttpMethod)
    .ThenBy(e => e.Route)
    .ToList();
Console.Error.WriteLine($"WebApi endpoints: {webApiEndpoints.Count}");

var portalEndpoints = ExtractPortalEndpoints(portalRoot)
    .OrderBy(e => e.File)
    .ThenBy(e => e.Line)
    .ToList();
Console.Error.WriteLine($"Portal endpoints: {portalEndpoints.Count}");

var portalRouteSet = new HashSet<string>(
    portalEndpoints
        .Where(e => !string.IsNullOrWhiteSpace(e.HttpMethod) && !string.IsNullOrWhiteSpace(e.NormalizedRoute))
        .Select(e => $"{e.HttpMethod}:{e.NormalizedRoute}"),
    StringComparer.OrdinalIgnoreCase);

var missingEndpoints = webApiEndpoints
    .Where(e => !string.IsNullOrWhiteSpace(e.HttpMethod) &&
                !string.IsNullOrWhiteSpace(e.NormalizedRoute) &&
                !portalRouteSet.Contains($"{e.HttpMethod}:{e.NormalizedRoute}"))
    .OrderBy(e => e.Controller)
    .ThenBy(e => e.HttpMethod)
    .ThenBy(e => e.Route)
    .ToList();

var result = new
{
    GeneratedAt = DateTime.UtcNow,
    RepoRoot = repoRoot,
    Summary = new
    {
        WebApiEndpointCount = webApiEndpoints.Count,
        PortalCallCount = portalEndpoints.Count,
        MissingInPortalCount = missingEndpoints.Count
    },
    WebApiEndpoints = webApiEndpoints,
    PortalEndpoints = portalEndpoints,
    MissingInPortal = missingEndpoints
};

var jsonOptions = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    WriteIndented = true,
    Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
};

var json = JsonSerializer.Serialize(result, jsonOptions);

Console.WriteLine(json);

if (!string.IsNullOrWhiteSpace(outputPath))
{
    var directory = Path.GetDirectoryName(outputPath);
    if (!string.IsNullOrWhiteSpace(directory))
    {
        Directory.CreateDirectory(directory);
    }
    File.WriteAllText(outputPath, json, Encoding.UTF8);
    Console.Error.WriteLine($"Endpoint analysis written to: {outputPath}");
}

var currentDirOutput = Path.Combine(Environment.CurrentDirectory, "analysis_output.json");
try
{
    File.WriteAllText(currentDirOutput, json, Encoding.UTF8);
    File.WriteAllText(Path.Combine(Environment.CurrentDirectory, "analysis_output_length.txt"), json.Length.ToString(), Encoding.UTF8);
}
catch
{
    // ignore
}

return 0;

static List<WebApiEndpoint> ExtractWebApiEndpoints(string controllersRoot)
{
    var endpoints = new List<WebApiEndpoint>();
    foreach (var file in Directory.GetFiles(controllersRoot, "*.cs", SearchOption.AllDirectories))
    {
        var source = File.ReadAllText(file);
        var tree = CSharpSyntaxTree.ParseText(source);
        var root = tree.GetCompilationUnitRoot();

        foreach (var classDeclaration in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
        {
            var className = classDeclaration.Identifier.Text;
            if (!className.EndsWith("Controller", StringComparison.Ordinal))
            {
                continue;
            }

            var controllerName = className.Substring(0, className.Length - "Controller".Length);
            var classRouteTemplate = GetRouteTemplate(classDeclaration.AttributeLists);

            foreach (var method in classDeclaration.DescendantNodes().OfType<MethodDeclarationSyntax>())
            {
                var httpAttributes = method.AttributeLists
                    .SelectMany(al => al.Attributes)
                    .Where(attr => IsHttpMethodAttribute(attr))
                    .ToList();

                if (httpAttributes.Count == 0)
                {
                    continue;
                }

                var methodRouteTemplate = GetRouteTemplate(method.AttributeLists);
                var methodName = method.Identifier.Text;

                foreach (var httpAttr in httpAttributes)
                {
                    var httpMethod = GetHttpMethodFromAttribute(httpAttr);
                    var templateFromHttpAttr = GetRouteTemplateFromAttribute(httpAttr);
                    var effectiveMethodRoute = templateFromHttpAttr ?? methodRouteTemplate;

                    var fullRoute = CombineRoutes(classRouteTemplate, effectiveMethodRoute, controllerName, methodName);
                    var normalizedRoute = NormalizeRoute(fullRoute);

                    var lineSpan = tree.GetLineSpan(method.Span);
                    endpoints.Add(new WebApiEndpoint(
                        Controller: controllerName,
                        Action: methodName,
                        HttpMethod: httpMethod,
                        Route: fullRoute,
                        NormalizedRoute: normalizedRoute,
                        File: Path.GetRelativePath(controllersRoot, file).Replace('\\', '/'),
                        Line: lineSpan.StartLinePosition.Line + 1
                    ));
                }
            }
        }
    }

    return endpoints;
}

static List<PortalEndpoint> ExtractPortalEndpoints(string portalRoot)
{
    var endpoints = new List<PortalEndpoint>();
    var httpMethodMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["getasync"] = "GET",
        ["getfromjsonasync"] = "GET",
        ["getstringasync"] = "GET",
        ["postasync"] = "POST",
        ["postasjsonasync"] = "POST",
        ["putasync"] = "PUT",
        ["putasjsonasync"] = "PUT",
        ["deleteasync"] = "DELETE",
        ["patchasync"] = "PATCH",
        ["sendasync"] = "SEND"
    };

    foreach (var file in Directory.GetFiles(portalRoot, "*.cs", SearchOption.AllDirectories))
    {
        var source = File.ReadAllText(file);
        var tree = CSharpSyntaxTree.ParseText(source);
        var root = tree.GetCompilationUnitRoot();

        foreach (var invocation in root.DescendantNodes().OfType<InvocationExpressionSyntax>())
        {
            if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
            {
                continue;
            }

            var methodName = memberAccess.Name.Identifier.Text;
            if (!httpMethodMap.TryGetValue(methodName, out var httpMethod))
            {
                continue;
            }

            var caller = memberAccess.Expression.ToString();
            if (!IsLikelyHttpCaller(caller))
            {
                continue;
            }

            if (invocation.ArgumentList?.Arguments.Count == 0)
            {
                continue;
            }

            var firstArg = invocation.ArgumentList.Arguments[0].Expression;
            var rawRoute = ExtractRouteString(firstArg);
            if (rawRoute == null)
            {
                continue;
            }

            var normalizedRoute = NormalizeRoute(rawRoute);
            var lineSpan = tree.GetLineSpan(invocation.Span);

            endpoints.Add(new PortalEndpoint(
                HttpMethod: httpMethod == "SEND" ? DetectHttpMethodFromHttpRequest(invocation) ?? httpMethod : httpMethod,
                RouteExpression: rawRoute,
                NormalizedRoute: normalizedRoute,
                Caller: caller,
                File: Path.GetRelativePath(portalRoot, file).Replace('\\', '/'),
                Line: lineSpan.StartLinePosition.Line + 1
            ));
        }
    }

    return endpoints;
}

static string? GetRouteTemplate(SyntaxList<AttributeListSyntax> attributeLists)
{
    foreach (var attr in attributeLists.SelectMany(al => al.Attributes))
    {
        if (attr.Name.ToString().Contains("Route", StringComparison.OrdinalIgnoreCase))
        {
            var template = GetRouteTemplateFromAttribute(attr);
            if (!string.IsNullOrEmpty(template))
            {
                return template;
            }
        }
    }

    return null;
}

static string? GetRouteTemplateFromAttribute(AttributeSyntax attribute)
{
    if (attribute.ArgumentList == null || attribute.ArgumentList.Arguments.Count == 0)
    {
        return null;
    }

    var firstArg = attribute.ArgumentList.Arguments[0].Expression;
    return ExtractRouteString(firstArg);
}

static bool IsHttpMethodAttribute(AttributeSyntax attribute)
{
    var name = attribute.Name.ToString();
    return name.Contains("HttpGet", StringComparison.OrdinalIgnoreCase)
           || name.Contains("HttpPost", StringComparison.OrdinalIgnoreCase)
           || name.Contains("HttpPut", StringComparison.OrdinalIgnoreCase)
           || name.Contains("HttpDelete", StringComparison.OrdinalIgnoreCase)
           || name.Contains("HttpPatch", StringComparison.OrdinalIgnoreCase)
           || name.Contains("HttpHead", StringComparison.OrdinalIgnoreCase)
           || name.Contains("HttpOptions", StringComparison.OrdinalIgnoreCase);
}

static string GetHttpMethodFromAttribute(AttributeSyntax attribute)
{
    var name = attribute.Name.ToString().ToLowerInvariant();
    if (name.Contains("httpget")) return "GET";
    if (name.Contains("httppost")) return "POST";
    if (name.Contains("httpput")) return "PUT";
    if (name.Contains("httpdelete")) return "DELETE";
    if (name.Contains("httppatch")) return "PATCH";
    if (name.Contains("httphead")) return "HEAD";
    if (name.Contains("httpoptions")) return "OPTIONS";
    return "UNKNOWN";
}

static string CombineRoutes(string? classRoute, string? methodRoute, string controller, string action)
{
    var baseRoute = classRoute ?? string.Empty;
    var methodPart = methodRoute ?? string.Empty;

    string ReplaceTokens(string text)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;
        text = text.Replace("[controller]", controller, StringComparison.OrdinalIgnoreCase);
        text = text.Replace("[action]", action, StringComparison.OrdinalIgnoreCase);
        return text;
    }

    baseRoute = ReplaceTokens(baseRoute);
    methodPart = ReplaceTokens(methodPart);

    if (string.IsNullOrWhiteSpace(baseRoute))
    {
        return NormalizeSlashes(methodPart);
    }

    if (string.IsNullOrWhiteSpace(methodPart))
    {
        return NormalizeSlashes(baseRoute);
    }

    return NormalizeSlashes($"{baseRoute}/{methodPart}");
}

static string NormalizeRoute(string? route)
{
    if (string.IsNullOrWhiteSpace(route))
    {
        return string.Empty;
    }

    var withoutQuery = route.Split('?', '#')[0];
    withoutQuery = withoutQuery.Trim();
    withoutQuery = withoutQuery.Trim('/');
    withoutQuery = withoutQuery.Replace("//", "/");

    // Normalize parameter tokens {id:guid} -> {*}
    withoutQuery = Regex.Replace(withoutQuery, @"\{[^}]+\}", "{*}");
    // Normalize area prefix like ~/api
    if (withoutQuery.StartsWith("~/"))
    {
        withoutQuery = withoutQuery[2..];
    }

    return withoutQuery.ToLowerInvariant();
}

static string NormalizeSlashes(string value)
{
    if (string.IsNullOrEmpty(value))
    {
        return value;
    }

    return Regex.Replace(value, @"[\\/]+", "/");
}

static string? ExtractRouteString(ExpressionSyntax expression)
{
    switch (expression)
    {
        case LiteralExpressionSyntax literal when literal.IsKind(SyntaxKind.StringLiteralExpression):
            return literal.Token.ValueText;
        case InterpolatedStringExpressionSyntax interpolated:
            var sb = new StringBuilder();
            foreach (var content in interpolated.Contents)
            {
                switch (content)
                {
                    case InterpolatedStringTextSyntax text:
                        sb.Append(text.TextToken.ValueText);
                        break;
                    case InterpolationSyntax:
                        sb.Append("{*}");
                        break;
                }
            }
            return sb.ToString();
        default:
            return null;
    }
}

static bool IsLikelyHttpCaller(string caller)
{
    var lowered = caller.ToLowerInvariant();
    return lowered.Contains("apiclient") ||
           lowered.Contains("httpclient") ||
           lowered.Contains("client") ||
           lowered.Contains("restclient");
}

static string? DetectHttpMethodFromHttpRequest(InvocationExpressionSyntax invocation)
{
    // Detect HttpRequestMessage(HttpMethod.X, "api/...")
    if (invocation.ArgumentList?.Arguments.Count >= 1)
    {
        var firstArg = invocation.ArgumentList.Arguments[0].Expression;
        if (firstArg is ObjectCreationExpressionSyntax objectCreation &&
            objectCreation.Type.ToString().Contains("HttpRequestMessage", StringComparison.Ordinal))
        {
            if (objectCreation.ArgumentList?.Arguments.Count >= 1)
            {
                var httpMethodExpr = objectCreation.ArgumentList.Arguments[0].Expression.ToString();
                if (httpMethodExpr.Contains("HttpMethod.Get", StringComparison.OrdinalIgnoreCase)) return "GET";
                if (httpMethodExpr.Contains("HttpMethod.Post", StringComparison.OrdinalIgnoreCase)) return "POST";
                if (httpMethodExpr.Contains("HttpMethod.Put", StringComparison.OrdinalIgnoreCase)) return "PUT";
                if (httpMethodExpr.Contains("HttpMethod.Delete", StringComparison.OrdinalIgnoreCase)) return "DELETE";
                if (httpMethodExpr.Contains("HttpMethod.Patch", StringComparison.OrdinalIgnoreCase)) return "PATCH";
            }
        }
    }

    return null;
}

record WebApiEndpoint(
    string Controller,
    string Action,
    string HttpMethod,
    string Route,
    string NormalizedRoute,
    string File,
    int Line);

record PortalEndpoint(
    string HttpMethod,
    string RouteExpression,
    string NormalizedRoute,
    string Caller,
    string File,
    int Line);
