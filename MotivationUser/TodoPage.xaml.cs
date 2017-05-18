using MotivationUser.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MotivationUser
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TodoPage : ContentPage
    {
        private bool _isStopVisible;

        TodoItemManager manager;
        TodoItem currentItem;
        AzureDataService _azure = new AzureDataService();
        FacebookUser facebookUser = new FacebookUser();
        User currentUser = new User();
        string token;

        public TodoPage(string _token)
        {
            token = _token;
           
            InitializeComponent();
        }
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if(token == "")
            {
                await Navigation.PushModalAsync(new LoginPage());
                token = App.Token;
            } else
            {
                await getFBInfo(token);
                if (!String.IsNullOrEmpty(facebookUser.Id))
                {
                    currentUser = _azure.GetUser(facebookUser.Id, "fbId");
                    App.PassData(currentUser.Id.ToString());
                   // App.SaveUserId(currentUser.Id.ToString());
                    if (currentUser == null)
                    {
                        _azure.RegisterUser(facebookUser.Name, facebookUser.Id);
                        currentUser = _azure.GetUser(facebookUser.Id, "fbId");
                        if (currentUser == null)
                            await Navigation.PushModalAsync(new LoginPage());
                    }
                
                }
                 refresh();
            }
            // Set syncItems to true in order to synchronize the data on startup when running in offline mode
            
            // await RefreshItems(true, syncItems: false);
        }
        void refresh()
        {
            DateTime? _dateTime = _azure.GetComplete(currentUser);
            int timeDiff;
            string cTime = DateTime.Now.ToString("HH:mm:ss");
            bool bDateTime = false;

            ChatGroup cGroup = _azure.GetGroup(currentUser.Id);
            double tDiff = 0.0;
            double lasttDiff = 0.0;
            var todo = _azure.GetTodo(cGroup);   

            if (_dateTime == null)
            {
                bDateTime = false;
            }               
            else
            {
                timeDiff = DateTime.Now.Day - _dateTime.Value.Day;
                if (timeDiff > 0)
                {
                    bDateTime = false;
                } else
                {
                    bDateTime = true;
                }               
            }
                

            if (bDateTime == false)
            {
                
               
                cGroup.toDos = todo.toDos;
                Debug.WriteLine("resfreshing rn");
                
                //Debug.WriteLine("refreshing rn ==" + info.FirstOrDefault());
                //DateTime dt = _azure.GetComplete(currentUser);
    


                    var scTime = TimeSpan.Parse(cTime).TotalSeconds;
                    foreach (var td in todo.toDos)
                    {
                        Debug.WriteLine("looking at time :" + td.SendTime);
                        var checkTime = TimeSpan.Parse(td.SendTime).TotalSeconds;
                        Debug.WriteLine("checkTime -" + checkTime);
                        Debug.WriteLine("scTime -" + scTime);
                        tDiff = scTime - checkTime;
                        if (tDiff > 0)
                        {
                            if (tDiff < lasttDiff)
                            {
                                currentItem = td;
                                Debug.WriteLine("lowest item -" + currentItem.ToDo);
                            }

                        }
                        lasttDiff = tDiff;
                    }
                    groupName.Text = cGroup.GroupName;
                    mustDo.Text = currentItem.ToDo;
                    toDoStack.IsVisible = true;
            } else
            {
                doneStack.IsVisible = true;
            }

        }
        void done_Toggled(object sender, EventArgs e)
        {
            _azure.CompleteTask(currentUser);
            toDoStack.IsVisible = false;
            doneStack.IsVisible = true;
        }
   
        public async Task getFBInfo(string accessToken)
        {
            var requestUrl = "https://graph.facebook.com/v2.8/me/"
                             + "?fields=name,picture,cover,age_range,devices,email,gender,is_verified"
                             + "&access_token=" + accessToken;
            var httpClient = new HttpClient();
            var userJson = await httpClient.GetStringAsync(requestUrl);
            facebookUser = JsonConvert.DeserializeObject<FacebookUser>(userJson);
        }
        public async void OnRefresh(object sender, EventArgs e)
        {
            var list = (ListView)sender;
            Exception error = null;
            try
            {
                await RefreshItems(false, true);
            }
            catch (Exception ex)
            {
                error = ex;
            }
            finally
            {
                list.EndRefresh();
            }

            if (error != null)
            {
                await DisplayAlert("Refresh Error", "Couldn't refresh data (" + error.Message + ")", "OK");
            }
        }
        private async Task RefreshItems(bool showActivityIndicator, bool syncItems)
        {
            using (var scope = new ActivityIndicatorScope(syncIndicator, showActivityIndicator))
            {
               // mustDo.Text = await manager.GetTodoItemAsync(syncItems);
            }
        }
        private class ActivityIndicatorScope : IDisposable
        {
            private bool showIndicator;
            private ActivityIndicator indicator;
            private Task indicatorDelay;

            public ActivityIndicatorScope(ActivityIndicator indicator, bool showIndicator)
            {
                this.indicator = indicator;
                this.showIndicator = showIndicator;

                if (showIndicator)
                {
                    indicatorDelay = Task.Delay(2000);
                    SetIndicatorActivity(true);
                }
                else
                {
                    indicatorDelay = Task.FromResult(0);
                }
            }

            private void SetIndicatorActivity(bool isActive)
            {
                this.indicator.IsVisible = isActive;
                this.indicator.IsRunning = isActive;
            }

            public void Dispose()
            {
                if (showIndicator)
                {
                    indicatorDelay.ContinueWith(t => SetIndicatorActivity(false), TaskScheduler.FromCurrentSynchronizationContext());
                }
            }

        }

      
    }
    
}
