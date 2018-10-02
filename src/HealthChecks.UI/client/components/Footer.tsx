import React from "react";

const Footer: React.SFC<any> = (props) => {
    return <div id="footer">
     <span>Xabaril Team @ {new Date().getFullYear()}</span>
    </div>
 }
        
export {Footer}