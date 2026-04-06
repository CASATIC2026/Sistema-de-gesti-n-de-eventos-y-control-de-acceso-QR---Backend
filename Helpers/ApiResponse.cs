/// <summary>
/// Clase para manejar las respuestas de la API.
/// </summary>
/// <typeparam name="T"></typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// Mensaje de la respuesta.
    /// </summary>
    public string Message { get; set; } = string.Empty;
    /// <summary>
    /// Datos de la respuesta.
    /// </summary>
    public T? Data { get; set; }
    /// <summary>
    /// Errores de la respuesta.
    /// </summary>
    public object? Errors { get; set; }
    /// <summary>
    /// Código de estado de la respuesta.
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Método para manejar las respuestas de éxito.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public static ApiResponse<T> Ok(T data, string message = "Operación exitosa")
    {
        return new ApiResponse<T>
        {
            Message = message,
            Data = data,
            StatusCode = 200
        };
    }

    /// <summary>
    /// Método para manejar las respuestas de error.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="statusCode"></param>
    /// <param name="errors"></param>
    /// <returns></returns>
    public static ApiResponse<T> Fail(string message, int statusCode = 400, object? errors = null)
    {
        return new ApiResponse<T>
        {
            Message = message,
            Errors = errors,
            StatusCode = statusCode
        };
    }
}