import React from "react";

export default function GoogleLoginButton({ returnUrl }) {
  const googleUrl = `/api/auth/google-login${
    returnUrl ? `?returnUrl=${encodeURIComponent(returnUrl)}` : ""
  }`;

  return (
    <a href={googleUrl} className="btn-auth-google">
      <img
        src="https://upload.wikimedia.org/wikipedia/commons/c/c1/Google_%22G%22_logo.svg"
        width="18"
        height="18"
        alt="Google"
      />
      через Google
    </a>
  );
}