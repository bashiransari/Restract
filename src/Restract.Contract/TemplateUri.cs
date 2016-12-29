namespace Restract.Contract
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class TemplateUri
    {
        private HashSet<string> _parameterNames;

        public string Template { get; set; }

        public HashSet<string> ParameterNames
        {
            get
            {
                if (_parameterNames == null)
                    ExtractTemplateParameters();
                return _parameterNames;
            }
        }

        public TemplateUri()
        {
        }

        public TemplateUri(string template) : this()
        {
            Template = template;
        }

        public string InjectParameterValues(Dictionary<string, string> parameterValues)
        {
            var url = Template;
            foreach (var parameterName in ParameterNames)
            {
                var parameterValue = parameterValues[parameterName];
                url = url.Replace($"{{{parameterName}}}", parameterValue);
            }

            var queryParameters = parameterValues.Where(p => !ParameterNames.Contains(p.Key));

            foreach (var parameterValue in queryParameters)
            {
                if (parameterValue.Value != null)
                {
                    url += url.Contains("?") ? "&" : "?";
                    url += parameterValue.Key + "=" + parameterValue.Value;
                }
            }

            return url;
        }

        public static TemplateUri Append(TemplateUri firstTemplateUri, params TemplateUri[] templateUris)
        {
            var template = firstTemplateUri.Template;
            foreach (var additionalTemplateUri in templateUris)
            {
                template = string.Concat(EnsureEndsWithSlash(template), additionalTemplateUri.Template);
            }
            var templateUri = new TemplateUri(template);
            return templateUri;
        }

        private static string EnsureEndsWithSlash(string url)
        {
            return (url.EndsWith("/") ? url : url + "/");
        }

        private void ExtractTemplateParameters()
        {
            var parameters = new HashSet<string>();
            var regex = new System.Text.RegularExpressions.Regex("{([^}]*)}");

            var matches = regex.Matches(Template);

            for (var i = 0; i < matches.Count; i++)
            {
                if (matches[i].Groups.Count == 2)
                {
                    var param = matches[i].Groups[1];
                    if (parameters.Contains(param.Value))
                    {
                        throw new Exception($"Duplicate parameters found in uri template: {Template}, parameter name: {param.Value}");
                    }

                    parameters.Add(param.Value);
                }
            }

            _parameterNames = parameters;
        }
    }
}