using CourseManager.API.DbContexts;
using CourseManager.API.Entities;
using CourseManager.API.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace CourseManager.API.Test
{
    public class AuthorRepositoryTests
    {
        private readonly ITestOutputHelper _output;

        public AuthorRepositoryTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void GetAuthors_PageSizeIsThree_ReturnsThreeAuthors()
        {
            // Arange
            var connectionStringBuilder = new SqliteConnectionStringBuilder { DataSource = ":memory:" };
            var connection = new SqliteConnection(connectionStringBuilder.ToString());
            var options = new DbContextOptionsBuilder<CourseContext>()
                .UseSqlite(connection)
                .Options;

            using (var context = new CourseContext(options))
            {
                context.Database.OpenConnection();
                context.Database.EnsureCreated();
                context.Countries.Add(new Entities.Country()
                {
                    Id = "BE",
                    Description = "Belgium"
                });
                context.Countries.Add(new Entities.Country()
                {
                    Id = "US",
                    Description = "United States of America"
                });


                context.Authors.Add(new Entities.Author()
                {
                    FirstName = "Kevin",
                    LastName = "Dockx",
                    CountryId = "BE"
                });
                context.Authors.Add(new Entities.Author()
                {
                    FirstName = "Gill",
                    LastName = "Cleeren",
                    CountryId = "BE"
                });
                context.Authors.Add(new Entities.Author()
                {
                    FirstName = "Julie",
                    LastName = "Lerman",
                    CountryId = "US"
                });
                context.Authors.Add(new Entities.Author()
                {
                    FirstName = "Shawn",
                    LastName = "Wildermuth",
                    CountryId = "BE"
                });
                context.Authors.Add(new Entities.Author()
                {
                    FirstName = "Debprah",
                    LastName = "Kurata",
                    CountryId = "US"
                });

                context.SaveChanges();
            }

            using (var context = new CourseContext(options))
            {
                var authorRepository = new AuthorRepository(context);

                // Act
                var authors = authorRepository.GetAuthors(1, 3);

                // Assert
                Assert.Equal(3, authors.Count());
            }
        }

        [Fact]
        public void GetAuthor_EmptyGuid_ThrowsArgumentException()
        {
            // Arrange
            var connectionStringBuilder = new SqliteConnectionStringBuilder { DataSource = ":memory:" };
            var connection = new SqliteConnection(connectionStringBuilder.ToString());
            var options = new DbContextOptionsBuilder<CourseContext>()
                .UseSqlite(connection)
                .Options;

            using (var context = new CourseContext(options))
            {
                context.Database.OpenConnection();
                context.Database.EnsureCreated();
                var authorRepository = new AuthorRepository(context);

                // Assert
                Assert.Throws<ArgumentException>(
                    // Act
                    () => authorRepository.GetAuthor(Guid.Empty));

            }
        }

        [Fact]
        public void AddAuthor_AuthorWithoutCountryId_AuthorHasBeAsCountryId()
        {
            // Arrange
            var connectionStringBuilder = new SqliteConnectionStringBuilder { DataSource = ":memory:" };
            var connection = new SqliteConnection(connectionStringBuilder.ToString());
            var options = new DbContextOptionsBuilder<CourseContext>()
                .UseLoggerFactory(new LoggerFactory(
                    new[] { new LogToActionLoggerProvider((log) =>
                        {
                            _output.WriteLine(log);
                        }) }))
                .UseSqlite(connection)
                .Options;

            using (var context = new CourseContext(options))
            {
                context.Database.OpenConnection();
                context.Database.EnsureCreated();

                context.Countries.Add(new Entities.Country()
                {
                    Id = "BE",
                    Description = "Belgium"
                });

                context.SaveChanges();
            }

            using (var context = new CourseContext(options))
            {
                var authorRepository = new AuthorRepository(context);
                var authorToAdd = new Author()
                {
                    FirstName = "Kevin",
                    LastName = "Dockx",
                    Id = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd")
                };

                // Act
                authorRepository.AddAuthor(authorToAdd);
                authorRepository.SaveChanges();
            }
            using (var context = new CourseContext(options))
            {
                //Assert
                var authorRepository = new AuthorRepository(context);
                var addedAuthor = authorRepository.GetAuthor(Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"));
                Assert.Equal("BE", addedAuthor.CountryId);
            }
        }
    }
}
