using System.Collections;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace test.Helpers;

// Binds a List<T> field submitted alongside file uploads in a multipart/form-data
// request. Reads Request.Form/Request.Query directly (bypassing the value provider's
// Form-only filter that [FromForm] applies) so it works whether the client sends one
// field holding the whole JSON array (e.g. formData.append('Materials', JSON.stringify(materials)))
// or repeated keys each holding a single JSON object (how Swagger UI renders array-of-object
// fields on a multipart operation).
public class JsonFormDataModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var request = bindingContext.HttpContext.Request;
        var name = bindingContext.FieldName;

        var rawValues = new List<string>();
        if (request.HasFormContentType && request.Form.TryGetValue(name, out var formValues))
            rawValues.AddRange(formValues.Where(v => !string.IsNullOrWhiteSpace(v))!);
        if (rawValues.Count == 0 && request.Query.TryGetValue(name, out var queryValues))
            rawValues.AddRange(queryValues.Where(v => !string.IsNullOrWhiteSpace(v))!);

        if (rawValues.Count == 0)
            return Task.CompletedTask;

        bindingContext.ModelState.SetModelValue(name, string.Join(",", rawValues), string.Join(",", rawValues));

        var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var itemType = bindingContext.ModelType.GetGenericArguments()[0];

        try
        {
            if (rawValues.Count == 1 && rawValues[0].TrimStart().StartsWith('['))
            {
                var list = JsonSerializer.Deserialize(rawValues[0], bindingContext.ModelType, jsonOptions);
                bindingContext.Result = ModelBindingResult.Success(list);
                return Task.CompletedTask;
            }

            var listType = typeof(List<>).MakeGenericType(itemType);
            var result = (IList)Activator.CreateInstance(listType)!;
            foreach (var raw in rawValues)
                result.Add(JsonSerializer.Deserialize(raw, itemType, jsonOptions));

            bindingContext.Result = ModelBindingResult.Success(result);
        }
        catch (JsonException)
        {
            bindingContext.ModelState.TryAddModelError(name, $"Invalid JSON format for {name}.");
        }

        return Task.CompletedTask;
    }
}
