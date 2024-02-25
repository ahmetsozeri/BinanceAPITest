using RestSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BinanceAPITest.Forms
{
    public partial class CoinListForm : Form
    {
      
        private string baseUrl = "https://api.binance.com";
        private int currentPage = 1;
        private int pageSize = 10; // Belirlediğiniz sayfa boyutu

        public CoinListForm()
        {
            InitializeComponent();
            LoadData();
        }

        private void CoinListForm_Load(object sender, EventArgs e)
        {
            LoadData();
        }


        private void LoadData()
        {
            try
            {
                var client = new RestClient(baseUrl);
                var request = new RestRequest("/api/v3/exchangeInfo", Method.Get);

                var response = client.Execute<ExchangeInfoResponse>(request);

                if (response.IsSuccessful)
                {
                    var exchangeInfo = response.Data;

                    // Extract coin data from the response
                    List<Coin> coinList = new List<Coin>();

                    var pageSize = 100; // Örnek olarak 100
                    var totalPages = (int)Math.Ceiling((double)exchangeInfo.Symbols.Count / pageSize);

                    for (int page = 1; page <= totalPages; page++)
                    {
                        var symbolsOnPage = exchangeInfo.Symbols.Skip((page - 1) * pageSize).Take(pageSize);

                        Parallel.ForEach(symbolsOnPage, symbolInfo =>
                        {
                            coinList.Add(new Coin
                            {
                                Symbol = symbolInfo.Symbol,
                                Price = GetCoinPrice(symbolInfo.Symbol)
                            });
                        });
                    }

                    // Perform pagination
                    var paginatedData = coinList.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();

                    // Bind the paginated data to DataGridView
                    dataGridView1.DataSource = paginatedData;

                    // Update navigation controls (page number, buttons, etc.)
                    // You may need to calculate the total number of pages based on the total number of coins
                }
                else
                {
                    MessageBox.Show("Error: " + response.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.ToString());
            }

        }

        private double GetCoinPrice(string symbol)
        {
            try
            {
                var priceRequest = new RestRequest("/api/v3/ticker/price", Method.Get);
                priceRequest.AddParameter("symbol", symbol);

                var priceResponse = new RestClient(baseUrl).Execute<TickerPriceResponse>(priceRequest);

                return priceResponse.IsSuccessful ? priceResponse.Data.Price : 0.0;
            }
            catch (Exception ex)
            {
                return 0;

            }

        }

        private void btnNextPage_Click(object sender, EventArgs e)
        {
            currentPage++;
            LoadData();
        }

        private void btnPrevPage_Click(object sender, EventArgs e)
        {
            if (currentPage > 1)
            {
                currentPage--;
                LoadData();
            }
        }

        // Define classes to represent API responses
        private class ExchangeInfoResponse
        {
            public List<SymbolInfo> Symbols { get; set; }
        }

        private class SymbolInfo
        {
            public string Symbol { get; set; }
        }

        private class TickerPriceResponse
        {
            public double Price { get; set; }
        }

        // Define a class to represent coin data
        private class Coin
        {
            public string Symbol { get; set; }
            public double Price { get; set; }
        }
    }
}
