using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using KioskManagement.Models;

public class KioskManagementDBEntities : DbContext
{
    public KioskManagementDBEntities()
    : base("name=KioskManagementDBEntities")
    {
    }

    public DbSet<User> Users { get; set; }

    public DbSet<Customer> Customers { get; set; }

    public DbSet<Area> Areas { get; set; }

    public DbSet<Kiosk> Kiosks { get; set; }

    public DbSet<Service> Services { get; set; }

    public DbSet<Contract> Contracts { get; set; }

    public DbSet<Payment> Payments { get; set; }

    public DbSet<Project> Projects { get; set; }

    public DbSet<MaintenanceRequest>
        MaintenanceRequests
    { get; set; }
}