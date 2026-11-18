
using System.Net.Http.Json;

Console.OutputEncoding = System.Text.Encoding.UTF8;

using HttpClient client = new HttpClient();
client.BaseAddress = new Uri("http://localhost:5300");

Console.WriteLine("=================================");
Console.WriteLine("     LIBRARY CLIENT SYSTEM");
Console.WriteLine("=================================");

while (true)
{
    Console.WriteLine();
    Console.WriteLine("1. Xem danh sach sach");
    Console.WriteLine("2. Xem chi tiet sach");
    Console.WriteLine("3. Them sach moi");
    Console.WriteLine("4. Dat muon sach");
    Console.WriteLine("5. Xem chi tiet yeu cau dat muon");
    Console.WriteLine("6. Xem danh sach dat muon cua nguoi dung");
    Console.WriteLine("0. Thoat");

    Console.Write("Nhap lua chon: ");
    string choice = Console.ReadLine() ?? "";

    try
    {
        switch (choice)
        {
            case "1":
            {
                var response = await client.GetAsync("/books");
                await ShowResponse(response);
                break;
            }

            case "2":
            {
                Console.Write("Nhap ID sach: ");
                if (!int.TryParse(Console.ReadLine(), out int bookId))
                {
                    Console.WriteLine("ID khong hop le!");
                    break;
                }

                var response = await client.GetAsync($"/books/{bookId}");
                await ShowResponse(response);
                break;
            }

            case "3":
            {
                Console.Write("Nhap ten sach: ");
                string title = Console.ReadLine() ?? "";

                Console.Write("Nhap tac gia: ");
                string author = Console.ReadLine() ?? "";

                Console.Write("Nhap tong so ban: ");
                if (!int.TryParse(Console.ReadLine(), out int totalCopies))
                {
                    Console.WriteLine("So luong khong hop le!");
                    break;
                }

                var book = new
                {
                    Title = title,
                    Author = author,
                    TotalCopies = totalCopies
                };

                var response = await client.PostAsJsonAsync("/books", book);
                await ShowResponse(response);
                break;
            }

            case "4":
            {
                Console.Write("Nhap User ID: ");
                if (!int.TryParse(Console.ReadLine(), out int userId))
                {
                    Console.WriteLine("User ID khong hop le!");
                    break;
                }

                Console.Write("Nhap Book ID: ");
                if (!int.TryParse(Console.ReadLine(), out int requestedBookId))
                {
                    Console.WriteLine("Book ID khong hop le!");
                    break;
                }

                var reservation = new
                {
                    UserId = userId,
                    BookId = requestedBookId
                };

                var response = await client.PostAsJsonAsync(
                    "/reservations", reservation);

                await ShowResponse(response);
                break;
            }

            case "5":
            {
                Console.Write("Nhap Reservation ID: ");
                if (!int.TryParse(Console.ReadLine(), out int reservationId))
                {
                    Console.WriteLine("ID khong hop le!");
                    break;
                }

                var response = await client.GetAsync(
                    $"/reservations/{reservationId}");

                await ShowResponse(response);
                break;
            }

            case "6":
            {
                Console.Write("Nhap User ID: ");
                if (!int.TryParse(Console.ReadLine(), out int searchUserId))
                {
                    Console.WriteLine("User ID khong hop le!");
                    break;
                }

                var response = await client.GetAsync(
                    $"/users/{searchUserId}/reservations");

                await ShowResponse(response);
                break;
            }

            case "0":
                Console.WriteLine("Da thoat chuong trinh!");
                return;

            default:
                Console.WriteLine("Lua chon khong hop le!");
                break;
        }
    }
    catch (HttpRequestException ex)
    {
        Console.WriteLine("Khong the ket noi den Server!");
        Console.WriteLine(ex.Message);
    }
    catch (TaskCanceledException)
    {
        Console.WriteLine("Yeu cau bi huy hoac het thoi gian cho!");
    }
    catch (Exception ex)
    {
        Console.WriteLine("Loi: " + ex.Message);
    }
}

async Task ShowResponse(HttpResponseMessage response)
{
    Console.WriteLine();
    Console.WriteLine("========== SERVER RESPONSE ==========");
    Console.WriteLine("HTTP Status: " + (int)response.StatusCode
        + " " + response.StatusCode);

    string content = await response.Content.ReadAsStringAsync();
    Console.WriteLine("Du lieu tra ve:");
    Console.WriteLine(content);

    Console.WriteLine("=====================================");
}
