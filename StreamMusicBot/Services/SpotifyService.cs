﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace StreamMusicBot.Services
{
    public class SpotifyService
    {
        private SpotifyClientConfig _clientConfig;
        private ClientCredentialsRequest _request;
        private ClientCredentialsTokenResponse _response;
        private SpotifyClient _client;

        private readonly LogService _logService;
        private readonly IConfiguration _config;

        public SpotifyService(LogService logService,
                              IConfiguration config)
        {
            _logService = logService;
            _config = config;
        }

        public async Task<SpotifyService> Init()
        {
            await SetNewSpotifyClient();
            return this;
        }

        public async Task<SpotifyClient> GetClient()
        {
            if (_response.IsExpired)
            {
                await SetNewSpotifyClient();
            }

            return _client;
        }

        private async Task SetNewSpotifyClient()
        {
            await _logService.LogAsync(new LogMessage(LogSeverity.Info, "SetNewSpotifyClient", "Spotify client token expired, requesting token.."));

            _clientConfig = SpotifyClientConfig.CreateDefault();
            _request = new ClientCredentialsRequest(_config["spotifyClientId"], _config["spotifyClientSecret"]);
            _response = await new OAuthClient(_clientConfig).RequestToken(_request);
            _client = new SpotifyClient(_clientConfig.WithToken(_response.AccessToken));
        }

    }
}
