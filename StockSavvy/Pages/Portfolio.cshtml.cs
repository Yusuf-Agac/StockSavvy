﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Bson;
using StockSavvy.Models;
using StockSavvy.Services;

namespace StockSavvy.Pages
{
	public class PortfolioModel : PageModel
    {
        [BindProperty]
        public List<StockMongoModel> stocks{ get; set; }
        public List<StockPriceModel> prices { get; set; }

        public void OnGet()
        {
            stocks = new List<StockMongoModel>();
            prices = new List<StockPriceModel>();

            var userName = Request.Cookies["username"];
            

            var UserService = new UserService();
            var user = UserService.GetOneByUsername(userName);

            var PortfolioService = new PortfolioService();
            var portfolio = PortfolioService.GetOneByUserId(user.Id);
            if (portfolio != null)
            {
                var stockIds = portfolio.Stocks;
                var StockService = new StockService();
                foreach (var id in stockIds)
                {
                    var stock = StockService.GetOneById(new ObjectId(id));
                    stocks.Add(stock);
                }
            }
        }
        
        private static string RandomString(int length)
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            char[] stringChars = new char[length];

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            return new string(stringChars);
        }
        
        public decimal GetStockPrice(string symbol)
        {
            string QUERY_URL = "https://www.alphavantage.co/query?function=GLOBAL_QUOTE&symbol="+symbol+"&apikey=" + RandomString(5);
            Uri queryUri = new Uri(QUERY_URL);

            using (WebClient client = new WebClient())
            {
                var json_data = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(client.DownloadString(queryUri));
                var data = json_data["Global Quote"];
                string priceString = data.GetProperty("05. price").GetString();
                if (decimal.TryParse(priceString, out decimal price))
                {
                    StockPriceModel stockPrice = new StockPriceModel();
                    stockPrice.StockCode = symbol;
                    stockPrice.Price = Convert.ToSingle(price);
                    prices.Add(stockPrice);
                    return price;
                }
                else
                {
                    return -1;
                }
            }
        }
    }
}