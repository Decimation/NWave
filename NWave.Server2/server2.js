const express = require('express');
const http = require('http');
const fs = require('fs');
const cors = require('cors');
const app = express();

app.use(cors());

app.get('/', cors(), (req, res) => {
	fs.readFile('index.html', (err, data) => {
		if (err) {
			res.statusCode = 500;
			res.end(`Error getting the file: ${err}.`);
		} else {
			res.statusCode = 200;
			res.setHeader('Content-Type', 'text/html');
			res.end(data);
		}
	});
});

app.listen(3000, 'localhost', () => {
	console.log('Server running at http://localhost:3000/');
});