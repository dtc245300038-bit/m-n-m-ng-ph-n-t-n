
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

// =====================================
// 1. KHỞI TẠO DỮ LIỆU SÁCH
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
// 2. KHỞI TẠO DỮ LIỆU ĐẶT MƯỢN
// =====================================

var reservations = new List<Reservation>();

// =====================================
// 3. API TRANG CHỦ
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
// 4. API LẤY DANH SÁCH SÁCH
// GET /books
// =====================================

app.MapGet("/books", () =>
{
    return Results.Ok(books);
});

// =====================================
// 5. API XEM CHI TIẾT SÁCH
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
// 6. API THÊM SÁCH
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
// 7. API TẠO YÊU CẦU ĐẶT MƯỢN
// POST /reservations
// =====================================

app.MapPost("/reservations", ([FromBody] CreateReservationRequest request) =>
{
    if (request == null || request.UserId <= 0 || request.BookId <= 0)
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

    reservations.Add(reservation);

    return Results.Created(
        $"/reservations/{reservation.Id}",
        reservation
    );
});

// =====================================
// 8. API XEM CHI TIẾT YÊU CẦU ĐẶT MƯỢN
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
// 9. API XEM YÊU CẦU CỦA NGƯỜI DÙNG
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
// 10. CHẠY SERVER
// =====================================

app.Run();

// =====================================
// 11. CÁC LỚP DỮ LIỆU
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
