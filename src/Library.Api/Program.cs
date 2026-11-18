
using Library.Api.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// =====================================
// 1. ĐĂNG KÝ SIGNALR
// =====================================

builder.Services.AddSignalR();

var app = builder.Build();

// Đăng ký địa chỉ SignalR Hub
app.MapHub<ReservationHub>("/reservationHub");

// =====================================
// 2. KHỞI TẠO DỮ LIỆU SÁCH
// =====================================

var books = new List<Book>
{
    new Book
    {
        Id = 1,
        Title = "Lap trinh C#",
        Author = "Nguyen Van A",
        TotalCopies = 10,
        AvailableCopies = 8
    },
    new Book
    {
        Id = 2,
        Title = "Lap trinh Java",
        Author = "Tran Thi B",
        TotalCopies = 5,
        AvailableCopies = 3
    },
    new Book
    {
        Id = 3,
        Title = "Co so du lieu",
        Author = "Le Van C",
        TotalCopies = 7,
        AvailableCopies = 7
    }
};

// =====================================
// 3. KHỞI TẠO DỮ LIỆU ĐẶT MƯỢN
// =====================================

var reservations = new List<Reservation>();

// =====================================
// 4. API TRANG CHỦ
// GET /
// =====================================

app.MapGet("/", () =>
{
    return Results.Ok(new
    {
        message = "Library API is running",
        version = "1.0"
    });
});

// =====================================
// 5. API LẤY DANH SÁCH SÁCH
// GET /books
// =====================================

app.MapGet("/books", () =>
{
    return Results.Ok(books);
});

// =====================================
// 6. API XEM CHI TIẾT SÁCH
// GET /books/{id}
// =====================================

app.MapGet("/books/{id:int}", (int id) =>
{
    var book = books.FirstOrDefault(b => b.Id == id);

    if (book == null)
    {
        return Results.NotFound(new
        {
            message = "Khong tim thay sach"
        });
    }

    return Results.Ok(book);
});

// =====================================
// 7. API THÊM SÁCH
// POST /books
// =====================================

app.MapPost("/books", ([FromBody] CreateBookRequest request) =>
{
    if (request == null ||
        string.IsNullOrWhiteSpace(request.Title) ||
        string.IsNullOrWhiteSpace(request.Author) ||
        request.TotalCopies <= 0)
    {
        return Results.BadRequest(new
        {
            message = "Du lieu sach khong hop le"
        });
    }

    var newBook = new Book
    {
        Id = books.Count == 0 ? 1 : books.Max(b => b.Id) + 1,
        Title = request.Title,
        Author = request.Author,
        TotalCopies = request.TotalCopies,
        AvailableCopies = request.TotalCopies
    };

    books.Add(newBook);

    return Results.Created(
        $"/books/{newBook.Id}",
        newBook
    );
});

// =====================================
// 8. API TẠO YÊU CẦU ĐẶT MƯỢN
// POST /reservations
// KẾT HỢP SIGNALR
// =====================================

app.MapPost("/reservations", async (
    [FromBody] CreateReservationRequest request,
    IHubContext<ReservationHub> hubContext) =>
{
    if (request == null ||
        request.UserId <= 0 ||
        request.BookId <= 0)
    {
        return Results.BadRequest(new
        {
            message = "Thong tin dat muon khong hop le"
        });
    }

    var book = books.FirstOrDefault(b => b.Id == request.BookId);

    if (book == null)
    {
        return Results.NotFound(new
        {
            message = "Khong tim thay sach"
        });
    }

    if (book.AvailableCopies <= 0)
    {
        return Results.BadRequest(new
        {
            message = "Sach hien khong con ban kha dung"
        });
    }

    var reservation = new Reservation
    {
        Id = reservations.Count == 0
            ? 1
            : reservations.Max(r => r.Id) + 1,

        UserId = request.UserId,
        BookId = request.BookId,
        Status = "Pending",
        CreatedAt = DateTime.Now
    };

    // Lưu yêu cầu đặt mượn
    reservations.Add(reservation);

    // Gửi thông báo thời gian thực đến nhóm người dùng
    await hubContext.Clients
        .Group($"user-{reservation.UserId}")
        .SendAsync("ReservationCreated", new
        {
            reservation.Id,
            reservation.UserId,
            reservation.BookId,
            reservation.Status,
            reservation.CreatedAt
        });

    return Results.Created(
        $"/reservations/{reservation.Id}",
        reservation
    );
});

// =====================================
// 9. API XEM CHI TIẾT YÊU CẦU ĐẶT MƯỢN
// GET /reservations/{id}
// =====================================

app.MapGet("/reservations/{id:int}", (int id) =>
{
    var reservation = reservations.FirstOrDefault(r => r.Id == id);

    if (reservation == null)
    {
        return Results.NotFound(new
        {
            message = "Khong tim thay yeu cau dat muon"
        });
    }

    return Results.Ok(reservation);
});

// =====================================
// 10. API XEM YÊU CẦU CỦA NGƯỜI DÙNG
// GET /users/{userId}/reservations
// =====================================

app.MapGet("/users/{userId:int}/reservations", (int userId) =>
{
    var userReservations = reservations
        .Where(r => r.UserId == userId)
        .ToList();

    return Results.Ok(userReservations);
});

// =====================================
// 11. API CẬP NHẬT TRẠNG THÁI ĐẶT MƯỢN
// PUT /reservations/{id}/status
// KẾT HỢP SIGNALR
// =====================================

app.MapPut("/reservations/{id:int}/status", async (
    int id,
    [FromBody] UpdateReservationStatusRequest request,
    IHubContext<ReservationHub> hubContext) =>
{
    // Kiểm tra dữ liệu đầu vào
    if (request == null ||
        string.IsNullOrWhiteSpace(request.Status))
    {
        return Results.BadRequest(new
        {
            message = "Trang thai khong hop le"
        });
    }

    // Tìm yêu cầu đặt mượn
    var reservation = reservations.FirstOrDefault(r => r.Id == id);

    if (reservation == null)
    {
        return Results.NotFound(new
        {
            message = "Khong tim thay yeu cau dat muon"
        });
    }

    // Các trạng thái được phép cập nhật
    string[] allowedStatuses =
    {
        "Reserved",
        "Rejected",
        "Borrowed",
        "Returned"
    };

    if (!allowedStatuses.Contains(request.Status))
    {
        return Results.BadRequest(new
        {
            message = "Trang thai khong duoc ho tro"
        });
    }

    // Cập nhật trạng thái
    reservation.Status = request.Status;

    // Gửi thông báo thời gian thực đến đúng người dùng
    await hubContext.Clients
        .Group($"user-{reservation.UserId}")
        .SendAsync("ReservationStatusChanged", new
        {
            reservation.Id,
            reservation.UserId,
            reservation.BookId,
            reservation.Status,
            reservation.CreatedAt
        });

    return Results.Ok(new
    {
        message = "Cap nhat trang thai thanh cong",
        reservation
    });
});

// =====================================
// 12. CHẠY SERVER
// =====================================

app.Run();

// =====================================
// 13. CÁC LỚP DỮ LIỆU
// =====================================

public class Book
{
    public int Id { get; set; }

    public string Title { get; set; } = "";

    public string Author { get; set; } = "";

    public int TotalCopies { get; set; }

    public int AvailableCopies { get; set; }
}

public class CreateBookRequest
{
    public string Title { get; set; } = "";

    public string Author { get; set; } = "";

    public int TotalCopies { get; set; }
}

public class Reservation
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int BookId { get; set; }

    public string Status { get; set; } = "Pending";

    public DateTime CreatedAt { get; set; }
}

public class CreateReservationRequest
{
    public int UserId { get; set; }

    public int BookId { get; set; }
}

// Lớp nhận dữ liệu cập nhật trạng thái
public class UpdateReservationStatusRequest
{
    public string Status { get; set; } = "";
}
