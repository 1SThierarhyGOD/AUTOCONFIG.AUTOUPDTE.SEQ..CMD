import { Component, OnInit } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { JwtHelperService } from '@auth0/angular-jwt';
import { ProgressSpinnerMode } from '@angular/material/progress-spinner';
import { UtilService } from './services/util.service';
import { environment } from '@app-env/environment';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css'],
})
export class AppComponent implements OnInit {

  apiResult: string | undefined;
  fullName: string | undefined;
  isSignedIn = false;
  jwtString: string | undefined; // To display JWT
  resultMessage: string | undefined;
  resultStatus: number | undefined;
  resultStatusText: string | undefined;
  ssoSignOnUrl = environment.samlSsoUrl;
  ssoSignOutUrl = environment.samlSloUrl;

  // Material progress spinner
  color = 'primary';
  mode: ProgressSpinnerMode = 'indeterminate';
  value = 60;
  isSpinnerVisible = false;

  constructor(
    private http: HttpClient,
    private util: UtilService,
    private jwtHelper: JwtHelperService
  ) { }

  ngOnInit() {

    // After SSO/SLO, return to original URL
    this.ssoSignOnUrl += `?returnurl=${document.location.origin}`;
    this.ssoSignOutUrl  += `?returnurl=${document.location.origin}`;

    // JWT is returned in a browser cookie
    var jwt = this.util.getCookieByName(environment.jwtCookieName);

    if (jwt) {

      // Set flag
      this.isSignedIn = true;

      // Decode the token, assign full name property
      const decodedJwt = this.jwtHelper.decodeToken(jwt);
      this.fullName = `${decodedJwt.given_name} ${decodedJwt.family_name}`;
      this.jwtString = JSON.stringify(decodedJwt);

      // For info/debug
      console.log(`JWT:\n${jwt}\nDecoded JWT:\n${this.jwtString}`);

    } else {
      // Set flag
      this.isSignedIn = false;

      console.log('JWT not found in browser cookie');
    }
  }

  // Button click event handler to call WebAPI
  onClick() {
    this.isSpinnerVisible = true;

    this.apiResult = "";
    this.resultStatus = undefined;
    this.resultStatusText = "";
    this.resultMessage = "";
    
    const url = `${environment.apiUrl}/samllicense`;

    this.apiResult = `Calling api "${url}". JWT will be sent in cookie...`;

    setTimeout(() => {
      this.http.get<any>(url, { withCredentials:true }).subscribe({
        next: (data) => {
          this.apiResult += " done.";
          this.resultStatus = 200;
          this.resultStatusText = "OK";
          this.resultMessage = data.displayMessage;
          this.isSpinnerVisible = false;
        },
        error: (err: HttpErrorResponse) => {  
          this.apiResult += "  done.";
          this.resultStatus = err.status;
          this.resultStatusText = err.statusText;
          this.resultMessage = "Error";
          this.isSpinnerVisible = false;
        }
    });
    }, 2000);
  }
}


