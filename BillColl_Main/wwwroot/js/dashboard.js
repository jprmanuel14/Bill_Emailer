let dateSort = "Newest - Oldest";
let pageNumber = 0;
let pageSize = 10;
let dateSent = "";

//let dateSortU = "Newest - Oldest";
let dateSortU = "";

let pageNumberU = 0;
let pageSizeU = 10;
let dateSentU = "";
let dateRangeU = "";

//let dateSortD = "Newest - Oldest";
let dateSortD = "";

let pageNumberD = 0;
let pageSizeD = 10;
let dateSentD = "";
let dateRangeD = "";

let totalpages = 0;
let currentpage = 1;

let clientName = "";
var selectedStartDateD, selectedEndDateD, selectedStartDateU, selectedEndDateU;

document.addEventListener("DOMContentLoaded", function () {
    //#jsonData is rendered by _Layout.cshtml. Sidebar visibility is decided server-side
    //there, so this only reads the payload; a missing or empty value must not throw.
    var jsonDataElement = document.getElementById('jsonData');

    if (jsonDataElement && jsonDataElement.value) {
        try {
            console.log("SETTING:", JSON.parse(jsonDataElement.value));
        }
        catch (e) {
            console.error('dashboard.js - could not parse #jsonData', e);
        }
    }
});

document.addEventListener('click', function handleClickOutsideBox(event) {
    // ??? the element the user clicked

    let target = event.target.classList.value
    let onClickElementByClass = document.getElementsByClassName('my-tooltip')

    let but = document.getElementById('button')





    if (target.includes('Appkit4-icon') || target.includes('my-tooltip-item')) {

        //
    }
    else {
        for (i = 0; i < onClickElementByClass.length; i++) {
            /*        */
            onClickElementByClass[i].style.display = "none"
        }
    }



});

//const myparam = pageNumber + "|" + pageSize + "|" + clientName + "|" + dateSent + "|" + dateSort;

//loadBillingList('/Home/GetPData', myparam, '#tblBodyDateStatus', '#tblFooterDateStatus', '.p-pagination', 3);

//const myparamU = pageNumberU + "|" + pageSizeU + "|" + clientName + "|" + dateSentU + "|" + dateSortU;

//const myparamD = pageNumberD + "|" + pageSizeD + "|" + clientName + "|" + dateSentD + "|" + dateSortD;

//loadBillingList('/Home/GetUData', myparamU, '#tblBodyUndelivered', '#tblFooterUndelivered', '.u-pagination', 2);

//loadBillingList('/Home/GetDData', myparamD, '#tblBodyDelivered', '#tblFooterDelivered', '.d-pagination', 1);

let picker = $('#DateSendOutURangeFilter').daterangepicker({
    locale: {
        format: 'DD MMMM YYYY'
    },
    "alwaysShowCalendars": true,
    autoApply: true,
    autoUpdateInput: true,
    "showCustomRangeLabel": false,
},
    function (start, end, label) {
        //
        // Lets update the fields manually this event fires on selection of range
        selectedStartDateU = start.format('DD MMMM YYYY'); // selected start
        selectedEndDateU = end.format('DD MMMM YYYY'); // selected end
    }
);

let picker2 = $('#DateStatementRangeFilter').daterangepicker({
    locale: {
        format: 'DD MMMM YYYY'
    },
    "alwaysShowCalendars": true,
    autoApply: true,
    autoUpdateInput: true,
    "showCustomRangeLabel": false,
},
    function (start, end, label) {
        //
        // Lets update the fields manually this event fires on selection of range
        selectedStartDateD = start.format('DD MMMM YYYY'); // selected start
        selectedEndDateD = end.format('DD MMMM YYYY'); // selected end
    }
);


$(function () {


    $('#DateSendOutURangeFilter').val('');
    $('#DateStatementRangeFilter').val('');

    //$("#tblmyUndelivered th").css("background-color", "white");
    //$("#tblmyUndelivered th").css("position", "relative");
    //$("#tblmyUndelivered th").css("z-index", "1000");
    //$("#tblmyUndelivered th").css("border-bottom", "1px solid black");

    var $th1 = $('.tableFixHeadU').find('thead th')
    $('.tableFixHeadU').on('scroll', function () {
        $th1.css('transform', 'translateY(' + this.scrollTop + 'px)');
    });




    var $th2 = $('.tableFixHeadD').find('thead th')
    $('.tableFixHeadD').on('scroll', function () {
        $th2.css('transform', 'translateY(' + this.scrollTop + 'px)');
    });


    $('#BtnSubmitReference').on('click', function () {

        pageNumber = 0;
        pageNumberD = 0;
        pageNumberU = 0;

        GetUploadListU();
        GetUploadListD();
        return false;
    });


    $(document).on('change', '#pageintervalU', function (event) {
        GetUploadListU();
    });

    $(document).on('change', '#pageintervalD', function (event) {
        GetUploadListD();
    });

    $(document).on('change', '#pageintervalP', function (event) {
        GetUploadListP();
    });

    function ClearRemarks() {
        if (!$('#StatusDetails').hasClass('deactivate')) {
            $('#StatusDetails').addClass('deactivate');
        }
        $('#StatusDetails').text('');
        $('#StatusDetailRemarks').text('');
    }

    function GetUploadListP() {
        ClearRemarks();
        //pageSize = $('#pageintervalP').val();
        console.log("1", pageNumber * 10)
        const myparam = (pageNumber * 10) + "|" + pageSize + "|" + clientName + "|" + dateSent + "|" + dateSort;
        loadBillingList('/Home/GetPData', myparam, '#tblBodyDateStatus', '#tblFooterDateStatus', '.p-pagination', 3);
    }

    function GetUploadListU() {

        let mydatefrom, mydateto;
        //pageSizeU = $('#pageintervalU').val();
        //pageNumberU = 0;
        clientName = $('#RefClientName').val();

        if ($('#DateSendOutURangeFilter').val() == '') {
            mydatefrom = '';
            mydateto = '';
            dateSentU = '';
        } else {
            mydatefrom = selectedStartDateU;
            mydateto = selectedEndDateU;
            dateSentU = mydatefrom + "-" + mydateto;
            //
        }

        const myparamU = (pageNumberU * 10) + "|" + pageSizeU + "|" + clientName + "|" + dateSentU + "|" + dateSortU;


        loadBillingList('/Home/GetUData', myparamU, '#tblBodyUndelivered', '#tblFooterUndelivered', '.u-pagination', 2);
    }

    function GetUploadListD() {

        let mydatefrom, mydateto;
        //pageSizeD = $('#pageintervalD').val();

        clientName = $('#RefClientName').val();

        if ($('#DateStatementRangeFilter').val() == '') {
            mydatefrom = '';
            mydateto = '';
            dateSentD = '';
        } else {
            mydatefrom = selectedStartDateD;
            mydateto = selectedEndDateD;
            dateSentD = mydatefrom + "-" + mydateto;
        }



        const myparamD = (pageNumberD * 10) + "|" + pageSizeD + "|" + clientName + "|" + dateSentD + "|" + dateSortD;

        loadBillingList('/Home/GetDData', myparamD, '#tblBodyDelivered', '#tblFooterDelivered', '.d-pagination', 1);
    }

    $('#btnApplyFilterDateSendOutU').on('click', function () {
        dateRangeU = $('#DateSendOutURangeFilter').val();
        dateSortU = $('input[name="DateSendOutUListValue"]:checked').val();

        if (typeof (selectedStartDateU) == "undefined") {
            selectedStartDateU = moment().format('DD MMMM YYYY'); // selected start
            selectedEndDateU = moment().format('DD MMMM YYYY'); // selected end
        }

        //

        if ((dateRangeU.trim() != "" && dateRangeU.trim() != "") || typeof (dateSortU) != "undefined" && dateSortU != "") {

            $('#DateSendOutUFilter').children().children('.filter-img').removeClass('disable');
            $('#DateSendOutUFilter').children().children('.filter-img').addClass('active');
        } else {
            dateRangeU = "";
            $('#DateSendOutUFilter').children().children('.filter-img').removeClass('active');
            $('#DateSendOutUFilter').children().children('.filter-img').addClass('disable');
        }

        $('#DateSendOutUFilterPanel').hide();

        GetUploadListU();
        AddFilterU();
        return false;
    });

    $('#btnApplyFilterDateStatement').on('click', function () {
        dateRangeD = $('#DateStatementRangeFilter').val();
        dateSortD = $('input[name="DateStatementListValue"]:checked').val();

        if (typeof (selectedStartDateD) == "undefined") {
            selectedStartDateD = moment().format('DD MMMM YYYY'); // selected start
            selectedEndDateD = moment().format('DD MMMM YYYY'); // selected end
        }


        if ((dateRangeD.trim() != "" && dateRangeD.trim() != "") || typeof (dateSortD) != "undefined" && dateSortD != "") {

            $('#DateStatementFilter').children().children('.filter-img').removeClass('disable');
            $('#DateStatementFilter').children().children('.filter-img').addClass('active');
        } else {
            dateRangeD = "";
            $('#DateStatementFilter').children().children('.filter-img').removeClass('active');
            $('#DateStatementFilter').children().children('.filter-img').addClass('disable');
        }

        $('#DateStatementFilterPanel').hide();

        GetUploadListD();
        AddFilterD();
        return false;
    });

    $(document).on('click', '.u-pagination a', function (event) {

        event.preventDefault();
        let mydatefrom, mydateto;

        var page = $(this).attr('href').split('list=')[1];
        if (typeof page == 'undefined') {
            return;
        }

        $('.u-pagination li').removeClass('active');
        $(this).parent('li').addClass('active');


        if ($('#DateSendOutURangeFilter').val() == '') {
            mydatefrom = '';
            mydateto = '';
        } else {
            mydatefrom = selectedStartDateU;
            mydateto = selectedEndDateU;
        }

        pageNumberU = page;

        const myparamU = pageNumberU + "|" + pageSizeU + "|" + clientName + "|" + dateSentU + "|" + dateSortU;
        loadBillingList('/Home/GetUData', myparamU, '#tblBodyUndelivered', '#tblFooterUndelivered', '.u-pagination', 2);

    });

    $(document).on('click', '.d-pagination a', function (event) {

        event.preventDefault();
        let mydatefrom, mydateto;

        var page = $(this).attr('href').split('list=')[1];
        if (typeof page == 'undefined') {
            return;
        }

        $('.d-pagination li').removeClass('active');
        $(this).parent('li').addClass('active');


        if ($('#DateStatementRangeFilter').val() == '') {
            mydatefrom = '';
            mydateto = '';
        } else {
            mydatefrom = selectedStartDateD;
            mydateto = selectedEndDateD;
        }

        pageNumberD = page;

        const myparamD = pageNumberD + "|" + pageSizeD + "|" + clientName + "|" + dateSentD + "|" + dateSortD;

        loadBillingList('/Home/GetDData', myparamD, '#tblBodyDelivered', '#tblFooterDelivered', '.d-pagination', 1);
    });

    $(document).on('click', '.p-pagination a', function (event) {
        //ClearRemarks();
        event.preventDefault();
        let mydatefrom, mydateto;

        var page = $(this).attr('href').split('list=')[1];
        if (typeof page == 'undefined') {
            return;
        }

        $('.p-pagination li').removeClass('active');
        $(this).parent('li').addClass('active');


        pageNumber = page

        console.log(pageNumber, "--> CHECK click period")


        const myparam = pageNumber + "|" + pageSize + "|" + clientName + "|" + dateSent + "|" + dateSort;
        loadBillingList('/Home/GetPData', myparam, '#tblBodyDateStatus', '#tblFooterDateStatus', '.p-pagination', 3);
    });

    $('#DateUploadRangeFilter').on('focusout', function () {
        $('#DateUploadRangeFilter').val('');
    });

    $('#DateStatementRangeFilter').on('focusout', function () {
        $('#DateStatementRangeFilter').val('');
    });


    function AddFilterU() {
        let myFilter = '';
        $('#myFiltersU').empty();

        if (dateRangeU != "" && typeof (dateRangeU) != "undefined") {

            myFilter += '<div class="filter-container"><div class="d-flex">Send out date: <div class="filter-items-2 ml-1" title="' + (dateRangeU) + '">' + (dateRangeU) + '</div><button id="removeDateUFilter" class="btn filter-dispose-btn" onclick="removeFilter(this)">x</button></div></div>';

        }
        if (dateSortU != "" && typeof (dateSortU) != "undefined") {

            myFilter += '<div class="filter-container"><div class="d-flex">Date sort: <div class="filter-items-2 ml-1" title="' + dateSortU + '">' + dateSortU + '</div><button id="removeDateUSortFilter" class="btn filter-dispose-btn" onclick="removeFilter(this)">x</button></div></div>';

        }
        $('#myFiltersU').append(myFilter);
    }

    function AddFilterD() {
        let myFilter = '';
        $('#myFiltersD').empty();

        if (dateRangeD != "" && typeof (dateRangeD) != "undefined") {

            myFilter += '<div class="filter-container"><div class="d-flex">Send out date: <div class="filter-items-2 ml-1" title="' + (dateRangeD) + '">' + (dateRangeD) + '</div><button id="removeDateDFilter" class="btn filter-dispose-btn" onclick="removeFilter(this)">x</button></div></div>';

        }
        if (dateSortD != "" && typeof (dateSortD) != "undefined") {

            myFilter += '<div class="filter-container"><div class="d-flex">Date sort: <div class="filter-items-2 ml-1" title="' + dateSortD + '">' + dateSortD + '</div><button id="removeDateDSortFilter" class="btn filter-dispose-btn" onclick="removeFilter(this)">x</button></div></div>';

        }
        $('#myFiltersD').append(myFilter);
    }

    $('#BtnDownload').on('click', function () {
        $('#BtnDownload2').trigger('click');

        setTimeout(generateDelivered, 2000);
    });

    $('#BtnDownload2').on('click', function () {
        generateUndelivered();
    });

    function generateDelivered() {
        let data = clientName + "|" + dateSentD;
        window.open("/Report/DownloadDelivered?file=" + data, '_blank');

    }

    function generateUndelivered() {
        let data2 = clientName + "|" + dateSentU;

        window.open("/Report/DownloadUndelivered?file=" + data2, '_blank');

    }

    //get 1st period

    function displayperiods() {

        console.log("DISPLAY: PAGE", pageNumber)

        return new Promise((resolve) => {
            const myparamlist = pageNumber + "|" + pageSize + "|" + clientName + "|" + dateSent + "|" + dateSort;

            //loadBillingList('/Home/GetPData', myparam, '#tblBodyDateStatus', '#tblFooterDateStatus', '.p-pagination', 3);



            let recentupload = "";
            let myparam = myparamlist.split("|");

            //let totalpages = 0;
            //let currentpage = 1;
            let currentrowmaxdata;
            let url = '/Home/GetPData';
            let tblbody = '#tblBodyDateStatus';
            let tblfooter = '#tblFooterDateStatus';
            let pagination_class = '.p-pagination';
            let module = 3;
            const maxDisplayRange = 5;

            $(tblbody).empty();
            $(tblfooter).empty();
            $.getJSON(url, { pageNumber: myparam[0], pageSize: myparam[1], clientName: myparam[2], dateSent: myparam[3], dateSort: myparam[4] }, function (data) {

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

                        break;
                    case 2:
                        frowname = '#firstrowU';
                        lrowname = '#lastrowU';
                        totalname = '#totaldataU';

                        $('#ctrU').text('(' + data.totalData + ')');

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


                if (data != '') {
                    //
                    let ctr = 1;
                    data.data.forEach((d) => {
                        const mydate = moment(d.statementDate).format('DD MMM YYYY');



                        switch (module) {
                            case 1:
                                //recentupload += '<tr><td>' + d.clientId + '</td><td>' + d.clientName + '</td><td>' + d.readReceipt + '</td><td>' + mydate + '</td><td>' + d.contactEmail + '<button id="d' + ctr + '" class="btn d-items" style="padding-top: 0;" onclick="ShowAdditionalInfo(this);"><div class="arrow-black down-arrow"></div></button></td>/tr>';

                                recentupload += '<tr><td>' + d.clientId + '</td><td>' + d.clientName + '</td><td>' + mydate + '</td><td>' + d.contactEmail + '<button id="d' + ctr + '" class="btn d-items" style="padding-top: 0;" onclick="ShowAdditionalInfo(this);"><div class="arrow-black down-arrow"></div></button></td>/tr>';
                                recentupload += '<tr id="d' + ctr + '-panel" class="d-items-bar" style="display:none;"><td colspan="5"><input type="hidden" class="refNo" value="' + d.eClientId + '"><div class="w-100">'
                                recentupload += '<div class="w-100 d-flex justify-content-end"><button class="btn btn-secondary-custom mr-2" onclick="SendMailDraft(this);">Resend</button> <button class="btn btn-secondary-custom mr-2" onclick="ShowMailDraft(this);" >View email</button></div>';

                                recentupload += '</td></tr>';
                                ctr++;
                                break;
                            case 2:
                                recentupload += '<tr><td>' + d.clientId + '</td><td>' + d.clientName + '</td><td>' + mydate + '</td><td>' + d.contactEmail + '<button id="u' + ctr + '" class="btn u-items" style="padding-top: 0;" onclick="ShowAdditionalInfo(this);"><div class="arrow-black down-arrow"></div></button></td></tr>';

                                recentupload += '<tr id="u' + ctr + '-panel" class="u-items-bar" style="display:none;"><td colspan="5"><input type="hidden" class="refNo" value="' + d.eClientId + '"><div class="w-100">'
                                //recentupload += '<div class="w-100 d-flex justify-content-between align-items-center pb-2"><div class="d-flex font-weight-bold align-items-center"><label>Action taken:</label><button class="btn btn-secondary-custom ms-2" onclick="NotifyContact(this);">Update</button></div><label class="font-weight-bold">Contact name: ' + d.contactName + '</label></div>';
                                recentupload += '<div class="w-100 d-flex justify-content-between align-items-center pb-2"><div class="d-flex font-weight-bold align-items-center"><label>Action taken:</label><button class="btn btn-secondary-custom ms-2" onclick="NotifyContact(this);">Update</button></div></div>';

                                recentupload += '<textarea class="undelivered-remarks w-100" readonly rows="5">' + d.remarks + '</textarea></div>';
                                recentupload += '<div class="w-100 d-flex justify-content-end"><button class="btn btn-secondary-custom mr-2" onclick="SendMailDraft(this);">Send</button> <button class="btn btn-secondary-custom mr-2" onclick="ShowMailDraft(this);" >View email</button> <button class="btn btn-secondary-custom" style="width:150px;" onclick="ShowSOADraft(this);">View statement</button></div>';
                                recentupload += '</td></tr>';
                                ctr++;
                                break;
                            default:
                                //Left table date sent and delivert rate 
                                recentupload += '<tr id="pdsent' + ctr + '"><td style="padding: 0px;"><a class="btn btn-td-link" href="" onclick="return filterByDate(this);">' + mydate + '</a></td><td style="padding: 0px;"><a class="btn btn-td-link" href="" onclick="return filterByDate(this);">' + d.deliveryRate + '%</a></td></tr>';
                                ctr++;
                                break;
                        }

                    })

                    let val = escapeHtml(recentupload)
                    $(tblbody).append(unescapeHtml(val)).html();

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

                if (currentpage + 1 == 1) {
                    myli += "<li class='page-item mx-1 disable' style='border:0'><span class='page-link'>" + '<span class="Appkit4-icon icon-left-chevron-outline"></span><span>Previous</span>' + "</span></li>"
                } else {
                    myli += "<li class='page-item  mx-1' style='border:0'><a class='page-link' href='" + url + (currentpage - 1) + "'>" + '<span class="Appkit4-icon icon-left-chevron-outline"></span><span>Previous</span>' + "</a></li>"
                }

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

                myli += `<div class='d-flex align-items-center justify-content-center'>
                              <div class='currentPage-container'>
                                <span>${currentpage + 1}</span>
                              </div>
                              <span class="ms-2">of</span>
                              <span class="ms-2">${totalpages}</span>
                         </div>  
                        `

                if (currentpage + 1 == totalpages || totalpages == 0) {
                    myli += "<li class='page-item mx-1 disable' style='border:0'><span class='page-link'>" + '<span>Next</span><span class="Appkit4-icon icon-right-chevron-outline"></span>' + "</span></li>"
                } else {
                    myli += "<li class='page-item  mx-1' style='border:0'><a class='page-link' href='" + url + (currentpage + 1) + "'>" + '<span>Next</span><span class="Appkit4-icon icon-right-chevron-outline"></span>' + "</a></li>"
                }

                let val = escapeHtml(myli)
                $(pagination_class).append(unescapeHtml(val)).html();

                return resolve("proceed");
            });

        })

    }

    selectDefault();

    async function selectDefault() {


        let mydata = await displayperiods();
        //
        if (mydata == "proceed") {
            let rowctr = $('table#tblDateStatus tbody tr:last').index() + 1;
            if (rowctr >= 1) {
                let tr = $('#tblDateStatus tbody tr:first');
                let myfrow = tr.find('td:eq(0) a');
                myfrow.trigger('click');

            }
        }
    }


});




function removeFilter(e) {

    $(e).parent().parent().remove();

    switch (e.id) {
        case "removeDateUFilter":
            $('#DateSendOutURangeFilter').val('');

            $('#btnApplyFilterDateSendOutU').trigger('click');

            break;
        case "removeDateDFilter":
            $('#DateStatementRangeFilter').val('');

            $('#btnApplyFilterDateStatement').trigger('click');

            break;
        case "removeDateUSortFilter":
            dateSortU = '';

            $('input[name="DateSendOutUListValue"]').each(function () {
                this.checked = false;
            });

            $('#btnApplyFilterDateSendOutU').trigger('click');

            break;
        default:
            dateSortD = '';

            $('input[name="DateStatementListValue"]').each(function () {
                this.checked = false;
            });

            $('#btnApplyFilterDateStatement').trigger('click');

            break;
    }

    if ($('#DateSendOutURangeFilter').val() == '' && $('#DateStatementRangeFilter').val() == '') {
        $('#StatusDetails').addClass('deactivate');
        $('#StatusDetails').text('');
        $('#StatusDetailRemarks').text('');
        $('#tblBodyDateStatus tr').removeClass('active-row-wc');

        $('#cDateSentSelectedRow').val('');
        $('#cDateSentSelectedPage').val('');
    }

}

function filterByDate(e) {
    let tr = $(e).closest('tr');
    let refno = tr.find('td:first-child .refNo').val();
    let mydate = tr.find('td:first-child').text();
    let myprate = tr.find('td:eq(1)').text();
    let myprateval = myprate.replace('%', '')
    //
    //

    //
    $('#cDateSentSelectedRow').val(tr.attr('id'));
    $('#cDateSentSelectedPage').val(pageNumber);

    $('#tblBodyDateStatus tr').removeClass('active-row-wc');

    tr.addClass('active-row-wc');

    $('#StatusDetails').text('');
    $('#StatusDetailRemarks').text('');
    if (myprateval == '100') {
        $('#StatusDetails').removeClass('deactivate');
        $('#StatusDetailRemarks').text('All auto sent emails during this period were successfully delivered.');
    } else {
        if (!$('#StatusDetails').hasClass('deactivate')) {
            $('#StatusDetails').addClass('deactivate');
        }

        let maxvalue = 100;
        let totalmxv = maxvalue - myprateval;

        $('#StatusDetails').text(Math.floor(totalmxv) + "%");
        $('#StatusDetailRemarks').text('of recent auto sent emails remain undelivered');
    }

    selectedStartDateD = moment(mydate).format('DD MMMM YYYY');
    selectedEndDateD = moment(mydate).format('DD MMMM YYYY');

    selectedStartDateU = moment(mydate).format('DD MMMM YYYY');
    selectedEndDateU = moment(mydate).format('DD MMMM YYYY');


    picker.data('daterangepicker').setStartDate(selectedStartDateU);
    picker.data('daterangepicker').setEndDate(selectedEndDateU);

    picker2.data('daterangepicker').setStartDate(selectedStartDateD);
    picker2.data('daterangepicker').setEndDate(selectedEndDateD);


    $('#DateStatementRangeFilter').val(selectedStartDateD + ' - ' + selectedEndDateD);
    $('#DateSendOutURangeFilter').val(selectedStartDateU + ' - ' + selectedEndDateU);
    //

    $('#btnApplyFilterDateStatement').trigger('click');
    $('#btnApplyFilterDateSendOutU').trigger('click');

    return false;
}


// PUT I T BACK
function ShowMailDraft(e) {
    let tr = $(e).closest('tr');
    let refno = tr.find('td:first-child .refNo').val();
    //
    window.open("/Report/pMail?reference=" + refno, '_blank');
}



function ShowSOADraft(e) {
    let tr = $(e).closest('tr');
    let refno = tr.find('td:first-child .refNo').val();



    window.open("/Report/pSOA?reference=" + refno, '_blank');
}

function SendMailDraft(e) {
    let tr = $(e).closest('tr');
    let refno = tr.find('td:first-child .refNo').val();
    let model = bindToken({ qstring: refno });
    $.post('/Home/SendMail', model, function (data) {

        const myparam = 0 + "|" + pageSize + "|" + clientName + "|" + dateSent + "|" + dateSort;

        const myparamU = 0 + "|" + pageSizeU + "|" + clientName + "|" + dateSentU + "|" + dateSortU;

        const myparamD = 0 + "|" + pageSizeD + "|" + clientName + "|" + dateSentD + "|" + dateSortD;


        loadBillingList('/Home/GetPData', myparam, '#tblBodyDateStatus', '#tblFooterDateStatus', '.p-pagination', 3);

        loadBillingList('/Home/GetUData', myparamU, '#tblBodyUndelivered', '#tblFooterUndelivered', '.u-pagination', 2);

        loadBillingList('/Home/GetDData', myparamD, '#tblBodyDelivered', '#tblFooterDelivered', '.d-pagination', 1);

        alert('Email successfully sent!');
    });

    location.reload();
    //
}

function ShowAdditionalInfo(e) {
    let myId = String(e.id).substring(0, 1);

    let targetId = e.id.replace('dt', '').replace('dn', '').replace('mt', '');

    if (myId == 'u') {
        mybodyclass = '#tblBodyUndelivered tr';

    } else {
        mybodyclass = '#tblBodyDelivered tr';
    }

    const mytableLength = $(mybodyclass).length;
    const tbodylengthVal = mytableLength / 2;
    for (var i = 1; i <= tbodylengthVal; i++) {

        if (myId + i != targetId) {
            $('#' + myId + i).children().removeClass('up-arrow');
            $('#' + myId + i).children().addClass('down-arrow');
            $('#' + myId + i + '-panel').slideUp('fast');
        }

    }


    if ($('#' + targetId).children().hasClass('down-arrow')) {
        //
        $('#' + targetId).children().removeClass('down-arrow');
        $('#' + targetId).children().addClass('up-arrow');
        $('#' + targetId + '-panel').slideDown();


    } else {

        $('#' + targetId).children().removeClass('up-arrow');
        $('#' + targetId).children().addClass('down-arrow');
        $('#' + targetId + '-panel').slideUp();
    }

}

const showContacts = (v) => {




    let onClickElementByClass = document.getElementsByClassName('my-tooltip')


    for (i = 0; i < onClickElementByClass.length; i++) {
        //
        onClickElementByClass[i].style.display = 'none';


    }

    let id = "tooltip" + v
    document.getElementById(id).style.display = 'block';



}

function NotifyContact(e) {
    let myValue = $(e).text();

    let tr = $(e).closest('tr');
    let udremarks = tr.find('td:first-child .undelivered-remarks');

    let refno = tr.find('td:first-child .refNo').val();

    //
    if (myValue == 'Update') {
        $(e).text('Save');

        udremarks.attr("readonly", false);

    } else {
        $(e).text('Update');

        let model = bindToken({ Remarks: udremarks.val(), Ref: refno });

        $.post('/Home/SaveRemarks', model, function (data) {

        });
        udremarks.attr("readonly", true);

    }

}


const viewStatement = (target, finalVal, index, client) => {


    // toggleMenu(v, index)
    // 



    let id = "tooltip" + index


    let onCLickElement = document.getElementById(id);


    onCLickElement.style.display = "none";


    let refno = client.clientId.toString() + '|' + client.statement_Date.toString()


    let protected = btoa(refno).replace(/=/g, '');



    window.open("/Report/pSOA?client_code_variable=" + client.clientId + '&statement_date_variable=' + client.statement_Date + '&los_variable=' + finalVal.oU_Code, '_blank');


    // $.getJSON('/view_reports_new', { 

    //         i_variable: index,
    //         Ex_statement_date_variable:"",
    //         ex_LOS_variable: finalVal.loS
    //     }, (res) => {

    //    

    // })


    // $('#stamentModal').modal('show');

    target.stopPropagation();
}

const viewEmail = (target, finalVal, index, client) => {


    // toggleMenu(v, index)
    // asdasd



    let id = "tooltip" + index


    let onCLickElement = document.getElementById(id);


    onCLickElement.style.display = "none";


    let refno = client.clientId.toString() + '|' + client.statement_Date.toString()


    let protected = btoa(refno).replace(/=/g, '');



    window.open("/Report/pMail?client_code_variable=" + client.clientId + '&statement_date_variable=' + client.statement_Date + '&los_variable=' + finalVal.oU_Code, '_blank');


    target.stopPropagation();
}

const viewTestEmail = (target, finalVal, index, client) => {

    // toggleMenu(v, index)
    // 



    let id = "tooltip" + index


    let onCLickElement = document.getElementById(id);


    onCLickElement.style.display = "none";


    let refno = client.clientId.toString() + '|' + client.statement_Date.toString()


    let protected = btoa(refno).replace(/=/g, '');



    window.open("/Home/SendMail_New?client_code_variable=" + client.clientId + '&statement_date_variable=' + client.statement_Date + '&los_variable=' + finalVal.loS, '_blank');


    target.stopPropagation();
}

function SendMail(target, finalVal, index, client, bool) {
    //let tr = $(e).closest('tr');
    //let refno = tr.find('td:first-child .refNo').val();
    //let model = bindToken({ qstring: refno });

    //window.open("/Home/SendMail_New?client_code_variable=" + client.clientId + '&statement_date_variable=' + client.statement_Date + '&los_variable=' + finalVal.loS, '_blank');


    $.post('/Home/SendMail', {
        client_code_variable: client.clientId,
        statement_date_variable: client.statement_Date,
        los_variable: finalVal.oU_Code,
        is_delivered: bool

        //OU_Code: finalVal.oU_Code

    }, function (data) {
        clientName = '';
        document.getElementById('RefClientName').value = ''
        //const myparam = pageNumber + "|" + pageSize + "|" + clientName + "|" + dateSent + "|" + dateSort;

        //const myparamU = pageNumberU + "|" + pageSizeU + "|" + clientName + "|" + dateSentU + "|" + dateSortU;

        //const myparamD = pageNumberD + "|" + pageSizeD + "|" + clientName + "|" + dateSentD + "|" + dateSortD;

        const myparam = 0 + "|" + pageSize + "|" + clientName + "|" + dateSent + "|" + dateSort;

        const myparamU = 0 + "|" + pageSizeU + "|" + clientName + "|" + dateSentU + "|" + dateSortU;

        const myparamD = 0 + "|" + pageSizeD + "|" + clientName + "|" + dateSentD + "|" + dateSortD;




        loadBillingList('/Home/GetPData', myparam, '#tblBodyDateStatus', '#tblFooterDateStatus', '.p-pagination', 3);

        loadBillingList('/Home/GetUData', myparamU, '#tblBodyUndelivered', '#tblFooterUndelivered', '.u-pagination', 2);

        loadBillingList('/Home/GetDData', myparamD, '#tblBodyDelivered', '#tblFooterDelivered', '.d-pagination', 1);


        alert('Email successfully sent!');
    });


    //location.reload();
    //
}
