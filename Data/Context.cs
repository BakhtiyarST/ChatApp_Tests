using ChatAppDB_Test.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatAppDB_Test.Data;

public class Context : DbContext
{
	public virtual DbSet<Message> Messages { get; set; }
	public virtual DbSet<User> Users { get; set; }
	public Context() { }
	protected override void OnConfiguring(DbContextOptionsBuilder optionBuilder)
		=> optionBuilder
			.LogTo(Console.WriteLine).UseLazyLoadingProxies()
			.UseNpgsql("Host=localhost;Username=postgres;Password=BorlandC++Builder6.0;Database=postgres");
	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<Message>(entity =>
		{
			entity.HasKey(x => x.Id).HasName("message_pkey");
			entity.ToTable("Messages");
			entity.Property(x => x.Id).HasColumnName("id");
			entity.Property(x => x.Text).HasColumnName("text");
			entity.Property(e => e.FromUserId).HasColumnName("from_user_id");
			entity.Property(e => e.ToUserId).HasColumnName("to_user_id");

			entity.HasOne(d => d.FromUser)
				.WithMany(p => p.FromMessages)
				.HasForeignKey(e => e.FromUserId)
				.HasConstraintName("message_from_user_id_fkey");

			entity.HasOne(d => d.ToUser)
				.WithMany(p => p.ToMessages)
				.HasForeignKey(e => e.ToUserId)
				.HasConstraintName("messages_to_user_id_fkey");
		});

		modelBuilder.Entity<User>(entity =>
		{
			entity.HasKey(x => x.Id).HasName("user_pkey");
			entity.ToTable("Users");
			entity.Property(x => x.Id).HasColumnName("id");
			entity.Property(x => x.Name)
				.HasMaxLength(255)
				.HasColumnName("name");
		});
		base.OnModelCreating(modelBuilder);
	}
	// partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
