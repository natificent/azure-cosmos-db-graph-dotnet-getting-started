using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace GraphGetStarted.Tests
{
    public class CrudTests
        : IDisposable
    {
        private PersonRepository _repo;
        private int _id;

        [Fact]
        public async void CreateAndRead()
        {
            _id = new Random().Next();

            var person = new Person
            {
                Id = _id,
                Firstname = $"TestFirstName{DateTime.Now.Millisecond}",
                Lastname = $"TestLastName{DateTime.Now.Millisecond}",
                Age = 40,
            };

            _repo = new PersonRepository();

            await _repo.AddAsync(person);


            var p = await _repo.GetAsync(_id);

            p.Firstname.ShouldBe(person.Firstname);
            p.Lastname.ShouldBe(person.Lastname);
            p.Id.ShouldBe(person.Id);
            p.Age.ShouldBe(person.Age);
        }

        public void Dispose()
        {
            _repo.DeleteAsync(_id).Wait();
            _repo.Dispose();
        }
    }
}
