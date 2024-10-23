require('dotenv').config();
const express = require('express');
const axios = require('axios');
const querystring = require('querystring');
const cookieParser = require('cookie-parser');

const app = express();
const port = 5000;

const client_id = process.env.SPOTIFY_CLIENT_ID;
const client_secret = process.env.SPOTIFY_CLIENT_SECRET;
const redirect_uri = process.env.REDIRECT_URI;

app.use(cookieParser());

const generateRandomString = (length) => {
  let text = '';
  const possible = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';

  for (let i = 0; i < length; i++) {
    text += possible.charAt(Math.floor(Math.random() * possible.length));
  }
  return text;
};

const stateKey = 'spotify_auth_state';

app.get('/login', (req, res) => {
  const state = generateRandomString(16);
  res.cookie(stateKey, state);

  const scope = 'user-read-email user-read-private user-top-read user-read-recently-played';
  const params = querystring.stringify({
    client_id: client_id,
    response_type: 'code',
    redirect_uri: redirect_uri,
    state: state,
    scope: scope,
  });

  res.redirect(`https://accounts.spotify.com/authorize?${params}`);
});

app.get('/callback', (req, res) => {
  const code = req.query.code || null;
  const state = req.query.state || null;
  const storedState = req.cookies ? req.cookies[stateKey] : null;

  if (state === null || state !== storedState) {
    res.redirect('/#' + querystring.stringify({ error: 'state_mismatch' }));
  } else {
    res.clearCookie(stateKey);

    axios({
      method: 'post',
      url: 'https://accounts.spotify.com/api/token',
      headers: {
        'Content-Type': 'application/x-www-form-urlencoded',
        Authorization: 'Basic ' + Buffer.from(`${client_id}:${client_secret}`).toString('base64'),
      },
      data: querystring.stringify({
        grant_type: 'authorization_code',
        code: code,
        redirect_uri: redirect_uri,
      }),
    })
      .then((response) => {
        if (response.status === 200) {
          const { access_token, refresh_token } = response.data;

          // Kullanıcı bilgilerini almak için Spotify API'ine istek
          axios.get('https://api.spotify.com/v1/me', {
            headers: {
              Authorization: `Bearer ${access_token}`,
            },
          })
          .then((response) => {
            // Kullanıcıyı frontend'e yönlendirme ve token'ı iletme
            res.redirect(`http://localhost:3000/dashboard?access_token=${access_token}`);
          })
          .catch((error) => {
            res.send(error);
          });
        } else {
          res.redirect('/#' + querystring.stringify({ error: 'invalid_token' }));
        }
      })
      .catch((error) => {
        res.send(error);
      });
  }
});

app.listen(port, () => {
  console.log(`Server running at http://localhost:${port}`);
});
