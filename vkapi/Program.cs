using System;
using System.Collections.Generic;
using System.Linq;
using vkapi;
using VkNet;
using VkNet.Enums.Filters;
using VkNet.Enums.SafetyEnums;
using VkNet.Model;
using VkNet.Model.RequestParams;
using Microsoft.EntityFrameworkCore;

namespace MyApp // Note: actual namespace depends on the project name.
{
    public class Program
    {

        //const string groupId = "73401175"; //25к members
        const string groupId = "9558750"; //11к members Галерея
        //const string groupId = "32169030"; //550к members neoclassic
        const int MAX_USER = 10000;
        public static void Main(string[] args)
        {
            var api = new VkApi();

            api.Authorize(new ApiAuthParams
            {
                AccessToken = "yourToken"
            });
            try
            {
                if (api.Groups.GetMembers(new GroupsGetMembersParams() { Count = 1, GroupId = groupId }) == null)
                    return;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }
            

            //добавляем группу в бд
            using(ApplicationContext db = new ApplicationContext())
            {
                vkapi.Models.Groups group = new vkapi.Models.Groups() { Id = groupId };

                if (!db.Groups.Contains(group))
                {
                    db.Groups.Add(group);
                    db.SaveChanges();
                }
                
            }
            //массив в который будем записывать User`ов
            List<User> userGroup = null;
            //общее кол-во людей в группе
            int userCount = 0;
            Console.WriteLine("Процесс пошёл");
            //В цикле перебираем всех юзеров
            do
            {
                //скролим 
                userGroup = api.Groups.GetMembers(new GroupsGetMembersParams() 
                {
                    GroupId = groupId, Count = 1000,
                    Offset = userCount, Fields = UsersFields.All
                }).ToList();
                userCount += userGroup.Count;
                Console.WriteLine("========================================" + userGroup.Count + "========================================");
            
               
                foreach (var user in userGroup)
                {
                    //сохранять в базе данных
                    using (ApplicationContext db = new ApplicationContext())
                    {
                        //обычная проверка на null, это С# , тут только так и делается
                        if (userGroup != null)
                        {
                            //валидация даты
                            if (user.BirthDate != null)
                            {
                                DateTime birthDay;
                                if(DateTime.TryParse(user.BirthDate, out birthDay))
                                {

                                    //test1
                                    
                                    var group = db.Groups.FirstOrDefault(g => g.Id == groupId);
            
                                    //проверка есть ли этот юзер вообще в бд, и есть ли у него эта группа
                                    var users = db.Users.Include(u => u.Groups).Select(u => u)
                                        .Where(u => u.Id == user.Id);
                                    
            
                                    //если есть в бд юзер
                                    if (users.Count() > 0)
                                    {
                                        var us = users.First();
                                        //у него нет этой группы
                                        if (us.Groups.Where(g => g.Id == groupId) == null)
                                        {
                                            if (group != null)
                                                us.Groups.Add(group);
                                        }
                                    }
                                    else
                                    {
                                        //создаём юзера
                                        var us = new vkapi.Models.Users();
            
                                        us.Id = user.Id;
                                        us.Birthday = birthDay.ToString();
                                        db.Users.Add(us);
                                        if (group != null)
                                            us.Groups.Add(group);
            
            
                                    }
            
                                    db.SaveChanges();
                                  
                                }
                            }
                        }
                    }
            
                    Console.WriteLine(user.FirstName + " " + user.LastName + " " + user.BirthDate + " " + user.Id);//просто посмотреть
            
                }
            
            } while (userGroup?.Count > 0 && userCount < MAX_USER);

            List<vkapi.Models.Users>? calcUsers = null;
            using(ApplicationContext db = new ApplicationContext())
            {
                
                List<vkapi.Models.Groups> groups = db.Groups.Include(g => g.Users).Select(g => g)
                    .Where(g => g.Id == groupId).ToList();
                if (groups.Count > 0)
                    calcUsers = groups.First().Users.ToList();
            }

            if(calcUsers != null)
            {
                Dictionary<string, string> probabilities = Calc(calcUsers, 1000, 100);
                if(probabilities != null)
                {
                    string pathToImage = DrawingChart.DrawChart(probabilities, groupId);
                }
                else
                {
                    Console.WriteLine("Слишком мало пользователей в группе");
                }
                
            }

            //Console.WriteLine(userCount);

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="users">список пользователей</param>
        /// <param name="precision">точность вычислений</param>
        /// <param name="countUser">от 0 до countUser рандомим</param>
        /// <returns>возвращает (ключ,значение) - (кол-во юзеров, вероятность совпадения)
        /// </returns>
        public static Dictionary<string, string> Calc(List<vkapi.Models.Users> users, int precision, int countUser)
        {
            countUser += 2;
            if(users.Count < countUser)
            {
                return null;
            }
            else
            {
                Dictionary<string, string> result = new Dictionary<string,string>();
                //внешний цикл от 2 до 100 (рандомим)
                for(int i = 2; i <= countUser; i++)
                {
                    int overlapBirthdayCount = 0;
                    //цикл сколько раз мы будем так делать
                    for (int j = 0; j < precision; j++)
                    {
                        //temp list
                        var tempUser = new List<vkapi.Models.Users>();
                        tempUser.AddRange((IEnumerable<vkapi.Models.Users>)users);
                        var randomUsers = new List<vkapi.Models.Users>();
                      

                        for(int k = 0; k < i; k++)
                        {
                            //Рандомим i пользователей(без повторениый)
                            int rand = new Random().Next(0, tempUser.Count);

                            //берём пользака и удаляем его из списка
                            randomUsers.Add(tempUser[rand]);
                            tempUser.RemoveAt(rand);
                        }
                        if (OverlapBirthday(randomUsers))
                        {
                            overlapBirthdayCount++;
                        }
                        
                    }
                    //вероятность
                    float chance = (float)overlapBirthdayCount / precision;
                    result.Add(i.ToString(), chance.ToString());
                }
                return result;

            }

        }

        public static bool OverlapBirthday(List<vkapi.Models.Users> users)
        {
            for(int i = 0; i < users.Count; i++)
            {
                for(int j = i + 1; j < users.Count; j++)
                {
                    var birthday1 = DateTime.Parse(users[i].Birthday);
                    var birthday2 = DateTime.Parse(users[j].Birthday);
                    if (birthday1.Date.Month == birthday2.Date.Month &&
                        birthday1.Date.Day == birthday2.Date.Day)
                        return true;
                }
            }
            return false;
        }

    }
}
