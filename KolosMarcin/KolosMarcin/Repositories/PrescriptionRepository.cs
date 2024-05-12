using System.Data.SqlClient;
using KolosMarcin.DTOs;
using KolosMarcin.Exceptions;
using KolosMarcin.Models;

namespace KolosMarcin.Repositories;

public class PrescriptionRepository : IPrescriptionRepository
{
    private readonly string connectionstring;

    public PrescriptionRepository(IConfiguration configuration)
    {
        connectionstring = configuration.GetConnectionString("DefaultConnection");
    }

   public async Task<List<Prescription>> GetPrescriptions(string? lastname)
   {
       var prescriptions = new List<Prescription>();

       using var con = new SqlConnection(connectionstring);
       await con.OpenAsync();

       string query;
       if (string.IsNullOrEmpty(lastname))
       {
           query = "SELECT * FROM Prescription order by DueDate DESC";
       }
       else
       {
           query = @"
            SELECT pr.IdPrescription, pr.Date, pr.DueDate, pr.IdPatient, pr.IdDoctor 
            FROM Prescription pr 
            INNER JOIN Patient p ON pr.IdPatient = p.IdPatient 
            WHERE p.LastName = @lastname
            order by DueDate DESC";
       }

       using (var cmd = new SqlCommand(query, con))
       {
           if (!string.IsNullOrEmpty(lastname))
           {
               cmd.Parameters.AddWithValue("@lastname", lastname);
           }

           try
           {
               using var reader = await cmd.ExecuteReaderAsync();
               if (await reader.ReadAsync())
               {
                   Prescription prescription = new()
                   {
                       IdPrescription = int.Parse(reader["IdPrescription"].ToString()),
                       Date = DateTime.Parse(reader["Date"].ToString()),
                       DueDate = DateTime.Parse(reader["DueDate"].ToString()),
                       IdPatient = int.Parse(reader["IdPatient"].ToString()),
                       IdDoctor = int.Parse(reader["IdDoctor"].ToString())
                   };
                   prescriptions.Add(prescription);
                   
               }
               return prescriptions;
           }
           catch (Exception ex)
           {
               // Obsługa błędów
               Console.WriteLine("Wystąpił błąd: " + ex.Message);
               throw;
           }
       }
   }


public async Task<int> AddMedicament(List<MedicamentDto> medicamentDtos, int id)
{
    using var con = new SqlConnection(connectionstring);
    await con.OpenAsync();

    using var transaction = con.BeginTransaction();

    try
    {
        foreach (var medicamentDto in medicamentDtos)
        {
            // Sprawdzenie, czy lek istnieje w bazie
            using (var cmd = new SqlCommand())
            {
                cmd.Connection = con;
                cmd.Transaction = transaction;
                cmd.CommandText = "SELECT COUNT(*) FROM Medicament WHERE Name = @name AND Description = @description AND Type = @type";
                cmd.Parameters.AddWithValue("@name", medicamentDto.Name);
                cmd.Parameters.AddWithValue("@description", medicamentDto.Description);
                cmd.Parameters.AddWithValue("@type", medicamentDto.Type);

                var result = await cmd.ExecuteScalarAsync();
                if ((int)result > 0)
                {
                    throw new NotFoundException("Nie ma takiego leku w bazie.");
                }
            }

            // Sprawdzenie, czy lek jest już przypisany do recepty
            using (var cmd = new SqlCommand())
            {
                cmd.Connection = con;
                cmd.Transaction = transaction;
                cmd.CommandText = "SELECT COUNT(*) FROM Prescription_Medicament WHERE IdMedicament IN (SELECT IdMedicament FROM Medicament WHERE Name = @name AND Description = @description AND Type = @type)";
                cmd.Parameters.AddWithValue("@name", medicamentDto.Name);
                cmd.Parameters.AddWithValue("@description", medicamentDto.Description);
                cmd.Parameters.AddWithValue("@type", medicamentDto.Type);

                var result = await cmd.ExecuteScalarAsync();
                if ((int)result > 0)
                {
                    throw new ConflictException("Ten lek jest już przypisany do recepty.");
                }
            }

            // Dodanie leku do recepty
            using (var cmd = new SqlCommand())
            {
                cmd.Connection = con;
                cmd.Transaction = transaction;
                cmd.CommandText = "INSERT INTO Prescription_Medicament(IdMedicament, IdPrescription) VALUES ((SELECT IdMedicament FROM Medicament WHERE Name = @name AND Description = @description AND Type = @type), @idPrescription)";
                cmd.Parameters.AddWithValue("@idPrescription", id);
                cmd.Parameters.AddWithValue("@name", medicamentDto.Name);
                cmd.Parameters.AddWithValue("@description", medicamentDto.Description);
                cmd.Parameters.AddWithValue("@type", medicamentDto.Type);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        transaction.Commit();
        return medicamentDtos.Count; // Zwróć liczbę dodanych leków
    }
    catch (DatabaseException)
    {
        transaction.Rollback();
        // Obsługa błędów związanych z bazą danych
        throw new DatabaseException("Błąd bazy danych podczas dodawania leku do recepty.");
    }
}

public async Task<int> AddMedicamentAsync(List<MedicamentToAddDto> medicamentToAddDtos, int id)
{
    using var con = new SqlConnection(connectionstring);
    await con.OpenAsync();
    using SqlTransaction transaction = (SqlTransaction)await con.BeginTransactionAsync();
    
    try
    {
        var query1 = "SELECT COUNT(*) FROM Prescription WHERE IdPrescription = @idPrescription";
        using (var cmd = new SqlCommand(query1, con, (SqlTransaction)transaction))
        {
            cmd.Parameters.AddWithValue("@idPrescription", id);
            var prescCount = (int)await cmd.ExecuteScalarAsync();
            if (prescCount == 0)
            {
                throw new NotFoundException("Nie ma takiej recepty.");
            }
        }

        foreach (var lek in medicamentToAddDtos)
        {
            var query2 = "SELECT COUNT(*) FROM Medicament WHERE IdMedicament = @idMedicament";
            using (var cmd = new SqlCommand(query2, con, (SqlTransaction)transaction))
            {
                cmd.Parameters.AddWithValue("@idMedicament", lek.idMedicament);
                var mediCount = (int)await cmd.ExecuteScalarAsync();
                if (mediCount == 0)
                {
                    throw new NotFoundException("Nie ma takiego leku.");
                }
            }
        }
      
        foreach (var lek in medicamentToAddDtos)
        {
            var query3 = @"
                INSERT INTO Prescription_medicament (IdMedicament, IdPrescription, Dose, Details) 
                VALUES (@idMedicament, @idPrescription, @dose, @details)";
            using (var cmd = new SqlCommand(query3, con, transaction))
            {
                cmd.Parameters.AddWithValue("@idMedicament", lek.idMedicament);
                cmd.Parameters.AddWithValue("@idPrescription", id);
                cmd.Parameters.AddWithValue("@dose", lek.dose);
                cmd.Parameters.AddWithValue("@details", lek.details);
                await cmd.ExecuteScalarAsync();
            }
        }

        // Zatwierdzenie transakcji
        await transaction.CommitAsync();

        return 1;
    }
    catch (Exception ex)
    {
        // Anulowanie transakcji w przypadku błędu
        await transaction.RollbackAsync();
        throw new Exception("Wystąpił błąd podczas dodawania leku.", ex);
    }
}
}