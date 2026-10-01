var urll = window.location.pathname.split('/');
removeClassLinks();

var myClientContactLimit;
//loading

$(document).ajaxStart(function () {
  document.getElementById("loader-panel").style.display = "flex";
});

$(document).ajaxStop(function () {
  document.getElementById("loader-panel").style.display = "none";
});


//document.addEventListener("DOMContentLoaded", function () {


//    console.log('nag load kaya eto')
//    var jsonDataElement = document.getElementById('jsonData');

//    console.log('awit??', jsonDataElement)

//    if (jsonDataElement) {
//        var jsonData = jsonDataElement.value;
//        var data = JSON.parse(jsonData);
//        localStorage.setItem("user", JSON.stringify(data));
//        // Use the data in your JavaScript code  
//        console.log(data, '--> called?');
//    } else {
//        console.error('JSON data element not found.');
//    }

//});

/**
 * Reads the user payload the layout renders into #jsonData.
 * Returns null when the element is absent or the payload is empty/malformed,
 * so a missing payload can never abort the rest of this file.
 */
const readUserContext = () => {
    var element = document.getElementById('jsonData');

    if (!element || !element.value) {
        return null;
    }

    try {
        var parsed = JSON.parse(element.value);
        return (parsed && typeof parsed === 'object') ? parsed : null;
    }
    catch (e) {
        console.error('site.js - could not parse #jsonData', e);
        return null;
    }
}

const isAdmin = () => {
    const user = readUserContext();

    // Treat an unreadable payload as "not admin": the Utilities link is only rendered
    // server-side for admins, so failing closed here matches what is already on the page.
    return !!user && String(user.User_Role) !== '0' && String(user.User_Role).trim() !== '';
}

/**
 * Guards navigation to a page that is only rendered for admins. The link itself is a real
 * anchor, so this is a convenience guard, not the access control.
 */
const validateAccess = (val) => {
    if (val !== 1) {
        return true;
    }

    if (!isAdmin()) {
        window.location = '/Home';
        return false;
    }

    return true;
}

if (urll[1] != '') {
    switch (urll[1]) {
        case "Clients":
            $('#side-nav-client').addClass('btn-active-link');
            $('.side-nav-client-ico').addClass('active');

            break;
        case "Settings":
            $('#side-nav-util').addClass('btn-active-link');
            $('.side-nav-util-ico').addClass('active');
     
            break;
        default:
            $('#side-nav-home').addClass('btn-active-link');
            $('.side-nav-home-ico').addClass('active');
            break;
    }
} else {
    //Home
    $('#side-nav-home').addClass('btn-active-link');
    $('.side-nav-home-ico').addClass('active');
}


function removeClassLinks() {
    $('#side-nav-home').removeClass('btn-active-link');
    $('.side-nav-home-ico').removeClass('active');

    $('#side-nav-util').removeClass('btn-active-link');
    $('.side-nav-util-ico').removeClass('active');

    $('#side-nav-client').removeClass('btn-active-link');
    $('.side-nav-client-ico').removeClass('active');

}

function openTab(e) {
  
    let i;
    let x = document.getElementsByClassName("sidebarTab");
    let y = document.getElementsByClassName('my-btn')
    for (i = 0; i < x.length; i++) {
        x[i].style.display = "none";
        y[i].classList.add("btn-side-panel-deactive-2");
    }

    document.getElementById(e).style.display = "block";
    document.getElementById(e + "btn").classList.remove("btn-side-panel-deactive-2");

    //Clients
    if (e == 'cContacts') {
        if ($('#tblmycContacts tbody tr').length >= myClientContactLimit) {
            $('#BtnAddContact').prop('disabled', true);
        } else {
            $('#BtnAddContact').prop('disabled', false);
        }
    }
    if (e == 'eContacts') {
        $('#BtnAddContact').prop('disabled', false);
    }


    return false;
}

function ViewFilter(e) {
    var newTextName = $(e).attr('name');
    var newTextNamePanel = newTextName + "Panel";
    if ($('#' + newTextNamePanel).is(':visible')) {
        $('#' + newTextNamePanel).hide();
    } else {
        $('#' + newTextNamePanel).show();
    }

    return false;
}




function loadBillingList(url, param, tblbody, tblfooter, pagination_class, module) {
    //Main table undelivered and delivered
    let recentupload = "";
    let myparamlist = String(param);
    let myparam = myparamlist.split("|");

    let totalpages = 0;
    let currentpage = 1;
    let currentrowmaxdata;

    let maxDisplayRange = 5;
    
    if (module == 3) {
        maxDisplayRange = 3;
    }

    $(tblbody).empty();
    $(tblfooter).empty();
    $.getJSON(url, { pageNumber: myparam[0], pageSize: myparam[1], clientName: myparam[2], dateSent: myparam[3], dateSort: myparam[4] }, function (data) {

      
        // document.getElementById("loader-panel").style.display = "none";

        console.log("RESULT: ", data)
        const interval = myparam[0];

        totalpages = data.totalPages;
        currentpage = data.currentPage;
        currentrowmaxdata = (data.currentPage * interval);

        let frowname = '';
        let lrowname = '';
        let totalname = '';
        
        
    
        switch (module) {
            case 1:
                frowname = '#firstrowD';
                lrowname = '#lastrowD';
                totalname = '#totaldataD';

                if (data.data.length != 0) {
                   
                    //$('#ctrD').text('(' + data.totalData + ')');
                    $('#ctrD').text('(' + data.data[0].delivered_Count + ')');
                }

                else {
                    $('#ctrD').text('(' + 0 + ')');
                }
               
                break;
            case 2:
                frowname = '#firstrowU';
                lrowname = '#lastrowU';
                totalname = '#totaldataU';

                if (data.data.length != 0) {
                    //$('#ctrU').text('(' + data.totalData + ')');
                    $('#ctrU').text('(' + data.data[0].undelivered_Count + ')');
                }
                else {
                    $('#ctrU').text('(' + 0 + ')');
                }

                break;
            default:
                frowname = '#firstrowP';
                lrowname = '#lastrowP';
                totalname = '#totaldataP';


                break;
        }

        if (totalpages > 0) {
            if (currentpage == 1) {
                $(frowname).text(currentpage);
            } else {
                $(frowname).text((parseInt(currentrowmaxdata) - parseInt(interval)) + 1);
            }

            if (currentpage == totalpages) {
                $(lrowname).text(data.totalData);
            } else {

                $(lrowname).text(parseInt(currentrowmaxdata));
            }
        } else {
            $(frowname).text(0);
            $(lrowname).text(0);
        }

        $(totalname).text(data.totalData);

        //Previous design sending email and viewing 11/28/2023 last change

        //<div class="w-100 d-flex align-items-center justify-content-between">

        //    <span class='a-text-label'>
        //        Comment:
        //    </span>

        //    <div class='d-flex align-items-center justify-content-center'>
        //        <span onclick="SendMailDraft(this);" class="Appkit4-icon icon-size icon-paperairplanefill"></span>

        //        <span onclick="ShowSOADraft(this);" class="Appkit4-icon icon-size icon-news-fill"></span>

        //        <span onclick="ShowMailDraft(this);" class="Appkit4-icon icon-size icon-email-fill"></span>
        //    </div>

        //</div>


        //recentupload += '<textarea class="undelivered-remarks w-100" readonly rows="5">' + d.remarks + '</textarea></div>';
        //recentupload += `<div class="w-100 d-flex justify-content-end align-items-center pb-2">
        //                                    <div class="d-flex font-weight-bold align-items-center">

        //                                        <button class="btn btn-primary-custom ms-2" onclick="NotifyContact(this);">
        //                                            Update
        //                                        </button>
        //                                     </div>
        //                                  </div>`;

         //Previous design sending email and viewing


        if (data != '') {
            //
            let ctr = 1;
            //protoype array join to remove commas try it when multiple contacts appear
            data.data.forEach((d, index) => {

                const mydate = d.statementDate != undefined ? moment(d.statementDate).format('DD MMM YYYY') : moment(d.statement_Date).format('DD MMM YYYY') 
              
                //case 1 is delivered case 2 is undelivered case 3 delivery date and rate

                
                switch (module) {
                    case 1:
                        // recentupload += '<tr><td>' + d.clientId + '</td><td>' + d.clientName + '</td><td>' + d.readReceipt + '</td><td>' + mydate + '</td><td>' + d.contactEmail + '<button id="d' + ctr + '" class="btn d-items" style="padding-top: 0;" onclick="ShowAdditionalInfo(this);"><div class="arrow-black down-arrow"></div></button></td>/tr>';

                        recentupload += '<tr><td style="width:130px; height:50px;"><button id="ddt' + ctr + '" class="btn d-items w-100 h-100" style="padding-top: 0;text-align:left;" onclick="ShowAdditionalInfo(this);">' + d.clientId + '</button></td><td style=" height:50px;"><button id="ddn' + ctr + '" class="btn d-items w-100 h-100" style="padding-top: 0;text-align:left;" onclick="ShowAdditionalInfo(this);">' + d.clientName + '</button></td><td style="height:50px;"><button id="dmt' + ctr + '" class="btn d-items w-100 h-100" style="padding-top: 0;text-align:left;" onclick="ShowAdditionalInfo(this);">' + mydate + '</button></td><td style="height:50px;"><button id="d' + ctr + '" class="btn d-items w-100 h-100" style="padding-top: 0;text-align:left;" onclick="ShowAdditionalInfo(this);"><div class="arrow-black down-arrow ms-2"></div></button></td>/tr>';
                        recentupload += '<tr id="d' + ctr + '-panel" class="d-items-bar" style="display:none;"><td colspan="5"><input type="hidden" class="refNo" value="' + d.eClientId + '"><div class="w-100">'
                        //recentupload += '<div class="w-100 d-flex justify-content-end"><button class="btn btn-secondary-custom mr-2" onclick="SendMailDraft(this);">Resend</button> <button class="btn btn-secondary-custom mr-2" onclick="ShowMailDraft(this);" >View email</button></div>';
                        recentupload += `
                                            <div class='w-100 rnz flex-column align-items-center justify-content-center'>
                                               
                                             <div class='w-100 rnz  d-flex align-items-center justify-content-center'>
                                                <table class='w-75'>
                                                    <thead>
                                                        <tr>
                                                            <th>
                                                               Operating unit
                                                            </th>
                                                              <th>
                                                                Primary contact 
                                                            </th>
                                                              <th>
                                                                <div class='d-flex align-items-center justify-content-center'>
                                                                    Actions
                                                                </div>
                                                              </th>
                                                            <th>
                                                            </th>
                                                        </tr>
                                                    </thead>
                                                    <tbody id="commentBody">
                                                        ${d.delivered_Insides.map((i, k) => {
                                                            let finalVal = JSON.stringify(i)
                                                            let clientDetails = JSON.stringify(d)
                                                            let combined = k + d.clientId + k + index + d.sent_Out_Date
                                                            finalVal['clientID'] = d.clientId

                                                            

                                                            return `
                                                         <tr >
                                                            <td>
                                                                <div class='p-3'>${i.ou}</div>
                                                            </td>
                                                            <td>
                                                                <div class='p-3'>${i.primary_Contact}</div>
                                                            </td>
                                                            <td>
                                                                 <button id='button' onclick='showContacts("${combined}")' class='d-flex align-items-center justify-content-center position-relative w-100 remove-button-style'>
                                                                    <span class="Appkit4-icon icon-vertical-more-outline font-weight-bold" style='font-size: 20px;'></span>

                                                                    <div  id='${"tooltip" + combined}' class='my-tooltip position-absolute  flex-column align-items-start' style='width: 200px; left: -110px;'>
                                                                            
                                                                
                                                                     ${ i.primary_Contact != "" ? `<div onclick='SendMail(event, ${finalVal}, "${combined}", ${clientDetails}, true)' class='my-tooltip-item'>  
                                                                                Resend email  
                                                                           </div>  ` : ''}

                                                                <div onclick='viewEmail(event, ${finalVal}, "${combined}", ${clientDetails})' class='my-tooltip-item'>
                                                                    View email
                                                               </div>

                                                               
                                                                    </button>
                                                                 </div>
                                                            </td>
                                                           
                                                        </tr>

                                                     
                                                            `
                                                                  }).join('')}
                                                    </tbody>
                                                </table>
                                               </div>

                                      

                                            </div>
                                        `;
                        recentupload += '</td></tr>';
                        ctr++;
                        break;
                    case 2:
                        //table items main table undelivered
                        let appendedHTML = ''; 

                        recentupload += '<tr><td style="width:130px; height:50px;"><button id="udt' + ctr + '" class="btn u-items w-100 h-100" style="padding-top: 0;text-align:left;" onclick="ShowAdditionalInfo(this);">' + d.clientId + '</button></td><td style=" height:50px;"><button id="udn' + ctr + '" class="btn u-items w-100 h-100" style="padding-top: 0;text-align:left;" onclick="ShowAdditionalInfo(this);">' + d.clientName + '</button></td><td style="height:50px;"><button id="umt' + ctr + '" class="btn u-items w-100 h-100" style="padding-top: 0;text-align:left;" onclick="ShowAdditionalInfo(this);">' + mydate + '</button></td><td style="height:50px;"><button id="u' + ctr + '" class="btn u-items w-100 h-100" style="padding-top: 0;text-align:left;" onclick="ShowAdditionalInfo(this);"><div class="arrow-black down-arrow ms-2"></div></button></td></tr>';

                        recentupload += '<tr id="u' + ctr + '-panel" class="u-items-bar" style="display:none;"><td colspan="5"><input type="hidden" class="refNo" value="' + d.eClientId + '"><div class="w-100">'
                        //recentupload += '<div class="w-100 d-flex justify-content-between align-items-center pb-2"><div class="d-flex font-weight-bold align-items-center"><label>Action taken:</label><button class="btn btn-secondary-custom ms-2" onclick="NotifyContact(this);">Update</button></div><label class="font-weight-bold">Contact name: ' + d.contactName + '</label></div>';
                        recentupload += `<div class='w-100 rnz flex-column align-items-center justify-content-center'>
                                             <div class='w-100 rnz d-flex align-items-center justify-content-center'>
                                                <table class='w-75'>
                                                    <thead>
                                                        <tr>
                                                            <th>
                                                                Operating unit
                                                            </th>
                                                              <th>
                                                                Primary contact
                                                            </th>
                                                              <th>
                                                                <div class='d-flex align-items-center justify-content-center'>
                                                                    Actions
                                                                </div>
                                                              </th>
                                                        </tr>
                                                    </thead>
                                                    <tbody id="commentBody">
                                                        ${d.undelivered_Insides.map((i, k) => {
                                                            let finalVal = JSON.stringify(i)
                                                            let bagoValue = d
                                                            bagoValue['clientName'] = d.clientName.replace("'", "");
                                                            
                                                            let clientDetails = JSON.stringify(bagoValue)
                                                            let combined = k + d.clientId + k + index
                                                            finalVal['clientID'] = d.clientId
                                                            finalVal['clientName'] = d.clientName.replace("'", "");
                                                            //for commits only
                                                            console.log(bagoValue,"please release", finalVal)
                                                            return `  
                                                         <tr >  
                                                            <td>  
                                                                <div class='p-3'>${i.ou}</div>  
                                                            </td>  
                                                            <td>  
                                                                <div class='p-3'>${i.primary_Contact}</div>  
                                                            </td>  
                                                            <td>  
                                                                 <button id='button' onclick='showContacts("${combined}")' class='d-flex align-items-center justify-content-center position-relative w-100 remove-button-style'>  
                                                                    <span class="Appkit4-icon icon-vertical-more-outline font-weight-bold" style='font-size: 20px;'></span>  
  
                                                                    <div  id='${"tooltip" + combined}' class='my-tooltip position-absolute  flex-column align-items-start' style='width: 200px; left: -110px;'>  
                      
                                                                           <div onclick='viewStatement(event, ${finalVal}, "${combined}", ${clientDetails})' class='my-tooltip-item'>  
                                                                                View statement  
                                                                           </div>  

                                                                          ${ i.primary_Contact != "" ? `<div onclick='SendMail(event, ${finalVal}, "${combined}", ${clientDetails}, false)' class='my-tooltip-item'>  
                                                                                Send email  
                                                                           </div>  ` : ''}

                                                                         
                   
                                                                            <div onclick='viewEmail(event, ${finalVal}, "${combined}", ${clientDetails})' class='my-tooltip-item'>  
                                                                                View email  
                                                                           </div>  
                                                                    </div>  
                                                                  </button>  
                                                            </td>  
                                                        </tr>`
                                                        }).join('')}  
                                                    </tbody>
                                                </table>
                                               </div>
                                            </div>
                                        `;
                      


                        recentupload += '</td></tr>';
                        ctr++;
                        break;
                    default:
                        recentupload += '<tr id="pdsent' + ctr + '"><td style="padding: 0px;"><a class="btn btn-td-link" href="" onclick="return filterByDate(this);">' + mydate + '</a></td><td style="padding: 0px;"><a class="btn btn-td-link" href="" onclick="return filterByDate(this);">' + d.deliveryRate + '%</a></td></tr>';
                        ctr++;
                        break;
                }

            })
            //Main table


            //removed commas



            let val = escapeHtml(recentupload);
            $(tblbody).append(unescapeHtml(val));

           
        }

        //Add highlight on existing row
        if (module == 3) {
            let dsentRowId = $('#cDateSentSelectedRow').val();
            let dsentPage = $('#cDateSentSelectedPage').val();

            if (dsentPage == myparam[0]) {
                $('#' + dsentRowId).addClass('active-row-wc');
            }
        }


        //Footer tblfooter
        $(tblfooter).empty();
        if (totalpages <= 0) {
            var rowval = "";
            switch (module) {
                case 1:
                    rowval = "<td colspan='4'><div class='w-100 d-flex justify-content-center'>No delivered emails</div></td>";
                    row = "<tr>" + rowval + "</tr>";
                    break;
                case 2:
                    rowval = "<td colspan='4'><div class='w-100 d-flex justify-content-center'>No undelivered emails</div></td>";
                    row = "<tr>" + rowval + "</tr>";
                    break;
                default:

                    rowval = "<td colspan='4'><div class='w-100 d-flex justify-content-center'>No data in the list</div></td>";
                    row = "<tr>" + rowval + "</tr>";
                    break;                  
            }

            $(tblfooter).append(row);
        }

        //Pagination
        $(pagination_class).empty();
        let url = '/MyPage?list=';
        let myli = '';

    
        let startPage;
        let endPage;

        if (totalpages <= maxDisplayRange) {
            startPage = 1;
            endPage = totalpages;
        } else {
            let maxPagesBeforeCurrentPage = Math.floor(maxDisplayRange / 2);
            let maxPagesAfterCurrentPage = Math.ceil(maxDisplayRange / 2) - 1;

            if (currentpage <= maxPagesBeforeCurrentPage) {
                // current page near the start
                startPage = 1;
                endPage = maxDisplayRange;
            } else if (currentpage + maxPagesAfterCurrentPage >= totalpages) {
                // current page near the end
                startPage = totalpages - maxDisplayRange + 1;
                endPage = totalpages;
            } else {
                // current page somewhere in the middle
                startPage = currentpage - maxPagesBeforeCurrentPage;
                endPage = currentpage + maxPagesAfterCurrentPage;
            }
        }

        //for (let i = startPage; i <= endPage; i++) {
        //    if (i == currentpage) {
        //        myli += "<li class='page-item active mx-1'><span class='page-link'>" + i + "</span></li>"
        //    } else {
        //        myli += "<li class='page-item  mx-1'><a class='page-link' href='" + url + i + "'>" + i + "</a></li>"
        //    }
        //}

        
        //Previousss
        if (currentpage + 1 == 1) {
            myli += "<li class='page-item mx-1 disable' style='border:0'><span class='page-link'>" + '<span class="Appkit4-icon icon-left-chevron-outline"></span><span>Previous</span>' + "</span></li>"
        } else {
        myli += "<li class='page-item  mx-1' style='border:0'><a class='page-link' href='" + url + (currentpage - 1) + "'>" + '<span class="Appkit4-icon icon-left-chevron-outline"></span><span>Previous</span>' + "</a></li>"
        }

        myli += `<div class='d-flex align-items-center justify-content-center'>
                               <div class='currentPage-container'>
                                <span>${currentpage + 1}</span>
                              </div>
                              <span class="ms-2">of</span>
                              <span class="ms-2">${totalpages}</span>
                        </div>`

        

         //Next
        if (currentpage + 1 == totalpages || totalpages == 0) {
            myli += "<li class='page-item mx-1 disable' style='border:0'><span class='page-link'>" + '<span>Next</span><span class="Appkit4-icon icon-right-chevron-outline"></span>' + "</span></li>"
        } else {
            myli += "<li class='page-item  mx-1' style='border:0'><a class='page-link' href='" + url + (currentpage + 1) + "'>" + '<span>Next</span><span class="Appkit4-icon icon-right-chevron-outline"></span>' + "</a></li>"
        }

        let val = escapeHtml(myli)

        $(pagination_class).append(unescapeHtml(val)).html();
    });
}



$(function () {

    //validateAccess(0)
    $.ajaxSetup({
        headers: {
            "RequestVerificationToken": $('input[name="RequestVerificationToken"]').val()
        }
    });



    $('#pwc-my-icon').on('click', function () {
        $('#myDashboardlist').hide();
        $('#sidebar').animate({ minWidth: "10px", maxWidth: "10px" });
        $('.sidebarContainer').addClass('sidebarContainer-active');
        document.getElementById("sidebarContainerId").addEventListener("click", resetDashboardSidebar);

        return false;
    })

    function resetDashboardSidebar() {
        $('#myDashboardlist').show();
        $('#sidebar').animate({ minWidth: "97px", maxWidth: "97px" });
        $('.sidebarContainer').removeClass('sidebarContainer-active');
        document.getElementById("sidebarContainerId").removeEventListener("click", resetDashboardSidebar);

        return false;
    }
});

function toggleMenu(v, index) {
    //let thispanel = $(e).parent().children('.' + e.id + 'Panel');
    ////let panelcustom = '#' + e.id + 'Panel';
    //if ($(thispanel).is(':visible')) {
    //    $(thispanel).hide();
    //} else {
    //    $(thispanel).show();
    //}
    //return false;

    //$('#ModalAddToExceptionConfirmation').modal('show');

    let id = "tooltip" + v.clientName + index

    let onCLickElement = document.getElementById(id);
    let onClickElementByClass = document.getElementsByClassName('my-tooltip')

 
   
    for (i = 0; i < onClickElementByClass.length; i++) {
        //
        onClickElementByClass[i].style.display = "none"
    }

    

    onCLickElement.style.display = "block"


}

function bindToken(data) {
    let newModelData = $.extend(data, { "__RequestVerificationToken": $('input[name="__RequestVerificationToken"]').val() });
    return newModelData;
}

populateClientContactLimit();

function populateClientContactLimit() {

    $.get("/Clients/GetClientContactLimit", function (data) {
        myClientContactLimit = data;
    });
}


const validateEmail = (email) => {
    return String(email)
        .toLowerCase()
        .match(
            /^(([^<>()[\]\\.,;:\s@"]+(\.[^<>()[\]\\.,;:\s@"]+)*)|.(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/
        );
};

function escapeHtml(html) {
    // var div =  document.createElement('div');
    // div.textContent = html;
    // return div.innerHTML;
    var escapeEl = document.createElement('div');
        escapeEl.textContent = html;
        return escapeEl.innerHTML;
    
    }

    function unescapeHtml(html) {
        var escapeEl = document.createElement('div');
        escapeEl.innerHTML = html;
        return escapeEl.textContent;
    }