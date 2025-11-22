using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ManoVecinaAPI.Data;
using ManoVecinaAPI.DTOs.Admin;
using ManoVecinaAPI.Models;

namespace ManoVecinaAPI.Controllers;

[ApiController]
[Route("admin")]
[Authorize(Roles = "admin")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _db;

    public AdminController(AppDbContext db)
    {
        _db = db;
    }

    // LISTAR TODOS LOS USUARIOS
    [HttpGet("users")]
    public IActionResult GetAllUsers()
    {
        var users = _db.Users
            .Select(u => new
            {
                u.Id,
                u.Name,
                u.Email,
                u.Role
            })
            .ToList();

        return Ok(users);
    }

    // CREAR USUARIO (solo admin)
    [HttpPost("create-user")]
    public IActionResult CreateUser([FromBody] CreateUserByAdminDto dto)
    {
        if (_db.Users.Any(u => u.Email == dto.Email))
            return Conflict(new { message = "El email ya está registrado." });

        var newUser = new User
        {
            Name = dto.Name,
            Email = dto.Email,
            Role = dto.Role,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
        };

        _db.Users.Add(newUser);
        _db.SaveChanges();

        return Ok(new { message = "Usuario creado exitosamente." });
    }

    // ELIMINAR USUARIO
    [HttpDelete("users/{id}")]
    public IActionResult DeleteUser(int id)
    {
        var user = _db.Users.FirstOrDefault(u => u.Id == id);
        if (user == null)
            return NotFound(new { message = "Usuario no encontrado." });

        _db.Users.Remove(user);
        _db.SaveChanges();

        return Ok(new { message = "Usuario eliminado correctamente." });
    }

    // LISTAR TRABAJADORES
    [HttpGet("workers")]
    public IActionResult GetWorkers()
    {
        var workers = _db.Users
            .Where(u => u.Role == "worker")
            .Select(u => new
            {
                u.Id,
                u.Name,
                u.Email,
                u.Latitude,
                u.Longitude,
                u.IsAvailable
            })
            .ToList();

        return Ok(workers);
    }

    // LISTAR CLIENTES
    [HttpGet("clients")]
    public IActionResult GetClients()
    {
        var clients = _db.Users
            .Where(u => u.Role == "client")
            .Select(u => new
            {
                u.Id,
                u.Name,
                u.Email
            })
            .ToList();

        return Ok(clients);
    }
}
