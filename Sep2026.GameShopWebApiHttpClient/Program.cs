using System.Net.Mime;
using System.Text;
using System.Text.Json;

using HttpClient httpClient = new HttpClient();
string port = "http://localhost:5194";
string path = @"C:\Users\AnyaWalker\Desktop\C#Training\C#Training\Sep2026.GameShopWebApiHttpClient\Responses";

// Get
HttpResponseMessage bookings = await httpClient.GetAsync($"{port}/api/booking");

if (bookings.IsSuccessStatusCode)
{
    string json = await bookings.Content.ReadAsStringAsync();
    //List<BookingResponse>? dto = JsonSerializer.Deserialize<List<BookingResponse>>(json);

    if (!string.IsNullOrWhiteSpace(json))
    {
        await File.WriteAllTextAsync(@$"{path}\booking.json", json);
    }
}
//get by Id
HttpResponseMessage bookingById = await httpClient.GetAsync($"{port}/api/booking/3003");
if (bookingById.IsSuccessStatusCode)
{
    string json = await bookingById.Content.ReadAsStringAsync();
    //BookingResponse? dto = JsonSerializer.Deserialize<BookingResponse>(json);

    if (!string.IsNullOrWhiteSpace(json))
    {
        await File.WriteAllTextAsync(@$"{path}\bookingById.json", json);
    }

}

//PostTime
DateTime startTime = DateTime.Now;
DateTime endTime = startTime.AddHours(2);
TimeSpan duration = endTime - startTime;
decimal totalHours = (decimal)duration.TotalHours;

BookingRequest bookingRequestDto = new BookingRequest
{
    consoleId = 1005,
    customerName = "Kaung Kaung",
    customerPhone = "09777344728",
    startTime = startTime,
    endTime = endTime,
    depositAmount = 5000,

};

string jsonContent = JsonSerializer.Serialize(bookingRequestDto);
StringContent content = new StringContent(jsonContent, Encoding.UTF8, MediaTypeNames.Application.Json);
HttpResponseMessage bookingCreateResponse = await httpClient.PostAsync($"{port}/api/booking", content);

if (bookingCreateResponse.IsSuccessStatusCode)
{
    string response = await bookingCreateResponse.Content.ReadAsStringAsync();
    if (!string.IsNullOrWhiteSpace(response))
    {
        await File.WriteAllTextAsync($@"{path}/bookingPostResponse.json", response);
    }


    //string response = JsonSerializer.Deserialize<>();
}

HttpResponseMessage deleteResponse = await httpClient.DeleteAsync(@$"{port}/api/booking/4002?isHardDelete=false");
if (deleteResponse.IsSuccessStatusCode)
{
    string response = await deleteResponse.Content.ReadAsStringAsync();
    Console.WriteLine(response);

}

//HttpResponseMessage bookingPostResponse = await 

public class BookingResponse
{
    public int Id { get; set; }
    public int ConsoleId { get; set; }
    public string CustomerName { get; set; }
    public string CustomerPhone { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public decimal TotalHours { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal DepositAmout { get; set; }
    public string bookingStatus { get; set; }
    public DateTime createdAt { get; set; }
    public DateTime updatedAt { get; set; }
    public bool isDeleted { get; set; }
}


public class BookingRequest
{
    public int consoleId { get; set; }
    public string customerName { get; set; }
    public string customerPhone { get; set; }
    public DateTime startTime { get; set; }
    public DateTime endTime { get; set; }
    public decimal depositAmount { get; set; }
}


public class BookingUpdateReqeust
{
    public string customerName { get; set; }
    public string customerPhone { get; set; }
    public DateTime startTime { get; set; }
    public DateTime endTime { get; set; }
    public decimal depositAmount { get; set; }
}

