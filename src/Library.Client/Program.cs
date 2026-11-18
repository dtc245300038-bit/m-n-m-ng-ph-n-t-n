
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR.Client;

Console.OutputEncoding = System.Text.Encoding.UTF8;

Console.WriteLine("=================================");
Console.WriteLine("     LIBRARY CLIENT SYSTEM");
Console.WriteLine("=================================");

// =====================================
// 1. NHAP USER ID
// =====================================

Console.Write("Nhap User ID cua ban: ");

if (!int.TryParse(Console.ReadLine(), out int currentUserId) ||
    currentUserId <= 0)
{
    Console.WriteLine("User ID khong hop le!");
    return;
}

// =====================================
// 2. KET NOI REST API
// =====================================

using HttpClient client = new HttpClient();

client.BaseAddress = new Uri("http://localhost:5300");

// =====================================
// 3. KET NOI SIGNALR
// =====================================

await using var connection = new HubConnectionBuilder()
    .WithUrl("http://localhost:5300/reservationHub")
    .WithAutomaticReconnect()
    .Build();

// =====================================
// 4. LANG NGHE THONG BAO THAM GIA NHOM
// =====================================

connection.On<string>("JoinedGroup", message =>
{
    Console.WriteLine();
    Console.WriteLine("[SIGNALR] " + message);
});

// =====================================
// 5. LANG NGHE THONG BAO DAT MUON SACH
// =====================================

connection.On<JsonElement>("ReservationCreated", data =>
{
    Console.WriteLine();
    Console.WriteLine("=================================");
    Console.WriteLine("   THONG BAO DAT MUON SACH MOI");
    Console.WriteLine("=================================");
    Console.WriteLine(data.ToString());
    Console.WriteLine("=================================");
});

// =====================================
// 6. LANG NGHE THONG BAO DOI TRANG THAI
// =====================================

connection.On<JsonElement>("ReservationStatusChanged", data =>
{
    Console.WriteLine();
    Console.WriteLine("=================================");
    Console.WriteLine("   THONG BAO CAP NHAT TRANG THAI");
    Console.WriteLine("=================================");
    Console.WriteLine(data.ToString());
    Console.WriteLine("=================================");
});

// =====================================
// 7. XU LY KET NOI LAI
// =====================================

connection.Reconnecting += error =>
{
    Console.WriteLine();
    Console.WriteLine("[SIGNALR] Mat ket noi. Dang thu ket noi lai...");
    return Task.CompletedTask;
};

connection.Reconnected += async connectionId =>
{
    Console.WriteLine();
    Console.WriteLine("[SIGNALR] Da ket noi lai!");
    Console.WriteLine("Connection ID: " + connectionId);

    await connection.InvokeAsync("JoinUserGroup", currentUserId);
};

connection.Closed += error =>
{
    Console.WriteLine();
    Console.WriteLine("[SIGNALR] Ket noi da dong.");
    return Task.CompletedTask;
};

// =====================================
// 8. KHOI DONG KET NOI SIGNALR
// =====================================

try
{
    await connection.StartAsync();

    Console.WriteLine();
    Console.WriteLine("[SIGNALR] Da ket noi thanh cong!");
    Console.WriteLine("Connection ID: " + connection.ConnectionId);

    await connection.InvokeAsync("JoinUserGroup", currentUserId);

    Console.WriteLine("Dang lang nghe thong bao cho User ID: "
        + currentUserId);
}
catch (Exception ex)
{
    Console.WriteLine("[SIGNALR] Khong the ket noi den Server!");
    Console.WriteLine(ex.Message);
    return;
}

// =====================================
// 9. MENU CHUC NANG
// =====================================

while (true)
{
    Console.WriteLine();
    Console.WriteLine("=================================");
    Console.WriteLine("     LIBRARY CLIENT SYSTEM");
    Console.WriteLine("     USER ID: " + currentUserId);
    Console.WriteLine("=================================");

    Console.WriteLine("1. Xem danh sach sach");
    Console.WriteLine("2. Xem chi tiet sach");
    Console.WriteLine("3. Them sach moi");
    Console.WriteLine("4. Dat muon sach");
    Console.WriteLine("5. Xem chi tiet yeu cau dat muon");
    Console.WriteLine("6. Xem danh sach dat muon cua nguoi dung");
    Console.WriteLine("7. Cap nhat trang thai dat muon");
    Console.WriteLine("0. Thoat");

    Console.Write("Nhap lua chon: ");

    string choice = Console.ReadLine() ?? "";

    try
    {
        switch (choice)
        {
            // =====================================
            // 1. XEM DANH SACH SACH
            // =====================================

            case "1":
            {
                var response = await client.GetAsync("/books");
                await ShowResponse(response);
                break;
            }

            // =====================================
            // 2. XEM CHI TIET SACH
            // =====================================

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

            // =====================================
            // 3. THEM SACH MOI
            // =====================================

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

            // =====================================
            // 4. DAT MUON SACH
            // =====================================

            case "4":
            {
                Console.WriteLine("User ID hien tai: " + currentUserId);

                Console.Write("Nhap Book ID: ");

                if (!int.TryParse(Console.ReadLine(), out int requestedBookId))
                {
                    Console.WriteLine("Book ID khong hop le!");
                    break;
                }

                var reservation = new
                {
                    UserId = currentUserId,
                    BookId = requestedBookId
                };

                var response = await client.PostAsJsonAsync(
                    "/reservations",
                    reservation
                );

                await ShowResponse(response);
                break;
            }

            // =====================================
            // 5. XEM CHI TIET YEU CAU DAT MUON
            // =====================================

            case "5":
            {
                Console.Write("Nhap Reservation ID: ");

                if (!int.TryParse(Console.ReadLine(), out int reservationId))
                {
                    Console.WriteLine("ID khong hop le!");
                    break;
                }

                var response = await client.GetAsync(
                    $"/reservations/{reservationId}"
                );

                await ShowResponse(response);
                break;
            }

            // =====================================
            // 6. XEM DANH SACH DAT MUON CUA NGUOI DUNG
            // =====================================

            case "6":
            {
                Console.Write("Nhap User ID: ");

                if (!int.TryParse(Console.ReadLine(), out int searchUserId))
                {
                    Console.WriteLine("User ID khong hop le!");
                    break;
                }

                var response = await client.GetAsync(
                    $"/users/{searchUserId}/reservations"
                );

                await ShowResponse(response);
                break;
            }

            // =====================================
            // 7. CAP NHAT TRANG THAI DAT MUON
            // =====================================

            case "7":
            {
                Console.Write("Nhap Reservation ID can cap nhat: ");

                if (!int.TryParse(Console.ReadLine(), out int updateReservationId))
                {
                    Console.WriteLine("Reservation ID khong hop le!");
                    break;
                }

                Console.WriteLine();
                Console.WriteLine("CAC TRANG THAI CO THE CHON:");
                Console.WriteLine("1. Reserved - Da xac nhan giu sach");
                Console.WriteLine("2. Rejected - Tu choi yeu cau");
                Console.WriteLine("3. Borrowed - Da muon sach");
                Console.WriteLine("4. Returned - Da tra sach");

                Console.Write("Nhap trang thai moi (1-4): ");

                string statusChoice = Console.ReadLine() ?? "";

                string newStatus = statusChoice switch
                {
                    "1" => "Reserved",
                    "2" => "Rejected",
                    "3" => "Borrowed",
                    "4" => "Returned",
                    _ => ""
                };

                if (string.IsNullOrEmpty(newStatus))
                {
                    Console.WriteLine("Lua chon trang thai khong hop le!");
                    break;
                }

                var statusRequest = new
                {
                    Status = newStatus
                };

                var response = await client.PutAsJsonAsync(
                    $"/reservations/{updateReservationId}/status",
                    statusRequest
                );

                await ShowResponse(response);
                break;
            }

            // =====================================
            // 0. THOAT
            // =====================================

            case "0":
            {
                Console.WriteLine("Dang ngat ket noi SignalR...");

                await connection.StopAsync();

                Console.WriteLine("Da thoat chuong trinh!");
                return;
            }

            default:
            {
                Console.WriteLine("Lua chon khong hop le!");
                break;
            }
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

// =====================================
// 10. HAM HIEN THI KET QUA SERVER
// =====================================

async Task ShowResponse(HttpResponseMessage response)
{
    Console.WriteLine();
    Console.WriteLine("========== SERVER RESPONSE ==========");

    Console.WriteLine(
        "HTTP Status: " +
        (int)response.StatusCode +
        " " +
        response.StatusCode
    );

    string content = await response.Content.ReadAsStringAsync();

    Console.WriteLine("Du lieu tra ve:");
    Console.WriteLine(content);

    Console.WriteLine("=====================================");
}
