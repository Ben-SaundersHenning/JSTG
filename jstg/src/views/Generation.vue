<script lang="ts" setup>

    import { ref, onMounted, watch, computed } from "vue";

    import { invoke } from "@tauri-apps/api/core";

    import { useForm } from "vee-validate";

    import { toTypedSchema } from "@vee-validate/zod";

    import { z } from "zod";

    import dayjs from 'dayjs';

    const includeAC = ref(false);
    const includeCAT = ref(false);
    const includeMRB = ref(false);

    const { errors, handleSubmit, defineField } = useForm({
        validationSchema: computed(() => toTypedSchema(
            z.object({
                assessorRegistrationId: z.string().regex(/^G[0-9]{7}$/),
                adjuster: z.string().optional(),
                insuranceCompany: z.string().trim().min(1),
                claimNumber: z.string().trim().min(1),
                referralCompanyId: z.number(),
                dateOfAssessment: z.string().date(),
                claimant: z.object({
                    firstName: z.string().trim().min(1),
                    lastName: z.string().trim().min(1),
                    gender: z.enum(["Male", "Female", "Other"]),
                    dateOfBirth: z.string().date(),
                    dateOfLoss: z.string().date(),
                    address: z.object({
                        streetAddress: z.string().trim().min(1),
                        unit: z.string().trim().min(1).optional(),
                        city: z.string().trim().min(1),
                        province: z.string().trim().min(1),
                        postalCode: z.string().trim().min(1),
                        country: z.string().trim().min(1),
                    }),
                }),
                documentId: z.number(),
                ac: (!(includeAC.value) ? z.optional(z.object({})) : z.discriminatedUnion("firstAssessment", [
                        z.object({
                            firstAssessment: z.literal(true),
                            dateOfLastAssessment: z.optional(z.string().date()),
                            monthlyAllowance: z.optional(z.string().trim().min(1)),
                        }),
                        z.object({
                            firstAssessment: z.literal(false),
                            dateOfLastAssessment: z.string().date(),
                            monthlyAllowance: z.string().trim().min(1),
                        }),
                    ])),
                cat: (!(includeCAT.value) ? z.optional(z.object({})) : z.object({
                            dateOfOcf19: z.string().date(),
                            assessor: z.string().trim().min(1)})),
                mrb: (!(includeMRB.value) ? z.optional(z.object({})) : z.object({
                            dateOfOcf18: z.string().date(),
                            assessor: z.string().trim().min(1),
                            amountOfOcf18: z.string().trim().min(1)})),
            }),
        )),
    });

    // NAMING SHORTCUTS
    // assessor => asr
    // referralCompany => rc
    // claimant => cl
    // address => add
    // dateOf => do
    const [asrRegistrationId, asrRegistrationIdAtrb] = defineField("assessorRegistrationId");
    const [adjuster, adjusterAtrb] = defineField("adjuster");
    const [insuranceCompany, insuranceCompanyAtrb] = defineField("insuranceCompany");
    const [claimNumber, claimNumberAtrb] = defineField("claimNumber");
    const [rcId, rcIdAtrb] = defineField("referralCompanyId");
    const [doAssessment, doAssessmentAtrb] = defineField("dateOfAssessment");
    const [clFirstName, clFirstNameAtrb] = defineField("claimant.firstName");
    const [clLastName, clLastNameAtrb] = defineField("claimant.lastName");
    const [clGender, clGenderAtrb] = defineField("claimant.gender");
    const [clDoBirth, clDoBirthAtrb] = defineField("claimant.dateOfBirth");
    const [clDoLoss, clDoLossAtrb] = defineField("claimant.dateOfLoss");
    const [clAddStreetAddress, clAddStreetAddressAtrb] = defineField("claimant.address.streetAddress");
    const [clAddUnit, clAddUnitAtrb] = defineField("claimant.address.unit");
    const [clAddCity, clAddCityAtrb] = defineField("claimant.address.city");
    const [clAddProvince, clAddProvinceAtrb] = defineField("claimant.address.province");
    const [clAddPostalCode, clAddPostalCodeAtrb] = defineField("claimant.address.postalCode");
    const [clAddCountry, clAddCountryAtrb] = defineField("claimant.address.country");
    const [documentId, documentIdAtrb] = defineField("documentId");
    const [acFirstAssessment, acFirstAssessmentAtrb] = defineField("ac.firstAssessment");
    const [acDateOfLastAssessment, acDateOfLastAssessmentAtrb] = defineField("ac.dateOfLastAssessment");
    const [acMonthlyAllowance, acMonthlyAllowanceAtrb] = defineField("ac.monthlyAllowance");
    const [catDateOfOcf19, catDateOfOcf19Atrb] = defineField("cat.dateOfOcf19");
    const [catAssessor, catAssessorAtrb] = defineField("cat.assessor");
    const [mrbDateOfOcf18, mrbDateOfOcf18Atrb] = defineField("mrb.dateOfOcf18");
    const [mrbAssessor, mrbAssessorAtrb] = defineField("mrb.assessor");
    const [mrbAmountOfOcf18, mrbAmountOfOcf18Atrb] = defineField("mrb.amountOfOcf18");

    // Example return formats:
    // July 23, 2024
    // December 25, 2019
    function formatDate(date) {
        const parsedDate = z.string().date().safeParse(date);
        if (parsedDate.success === true) {
            return dayjs(date).format('MMMM D, YYYY');
        } else {
            return "";
        }
    }

    let assessors = ref([
    ])

    let referral_companies = ref([
    ])

    let documents = ref([
    ])

    watch(documentId, (newID) => {

        includeAC.value = false;
        includeCAT.value = false;
        includeMRB.value = false;

        const doc = documents.value.find((document) => document.id === newID);
        const docTypes = doc.document.split(" ");
        if(docTypes.includes("AC")) {
            includeAC.value = true;
        }
        if(docTypes.includes("CAT") || docTypes.includes("CAT_GOSE")) {
            includeCAT.value = true;
        }
        if(docTypes.includes("MRB")) {
            includeMRB.value = true;
        }

    });

    function onSuccess(values) {

        // Remove potentially empty keys
        if (values["ac"] !== undefined && Object.keys(values["ac"]).length === 0) {
            delete values["ac"];
        }
        if (values["cat"] !== undefined && Object.keys(values["cat"]).length === 0) {
            delete values["cat"];
        }
        if (values["mrb"] !== undefined && Object.keys(values["mrb"]).length === 0) {
            delete values["mrb"];
        }

        // console.log(JSON.stringify(values));

        invoke('request_document', { data: JSON.stringify(values) });

    }

    function onInvalidSubmit({ values, errors, results }) {
        console.log(errors);
        // console.log("IncludeAC:", includeAC);
        // console.log("IncludeCAT:", includeCAT);
        // console.log("IncludeMRB:", includeMRB);
    }

    const onSubmit = handleSubmit(onSuccess, onInvalidSubmit);

    onMounted(() => {

        invoke('get_assessor_options').then((assessor_options) => {
            // console.log(assessor_options.listing_details);
            assessors.value = assessor_options.listing_details as Array;
        })
        .catch((e) => console.log(e));

        invoke('get_document_options').then((document_options) => {
            // console.log(document_options.listing_details);
            documents.value = document_options.listing_details as Array;
        })
        .catch((e) => console.log(e));

        invoke('get_referral_company_options').then((rc_options) => referral_companies.value = rc_options.listing_details as Array)
        .catch((e) => console.log(e));

    })

</script>

<template>
    <form @submit="onSubmit">

        <fieldset class="doc-gen-card">

            <legend class="card-title">Assessment Information</legend>
            <div class="assessment-inputs">
                <div class="assessor-input vertical-input">
                    <p class="input-label">Assessor</p>
                    <div class="horizontal-input input-border">
                        <span v-for="(assessor, index) in assessors">
                            <input type="radio" name="assessor" :id="'assessor' + assessor.id" :value="assessor.id"
                                   v-model="asrRegistrationId" :="asrRegistrationIdAtrb"/>
                            <label :for="'assessor' + assessor.id">{{assessor.name}}</label>
                        </span>
                    </div>
                    <span class="error">{{errors['assessorRegistrationId']}}</span>
                </div>
                <div class="document-input vertical-input">
                    <p class="input-label">Type</p>
                    <div class="checkboxes input-border">
                        <span v-for="(document, index) in documents">
                            <input type="radio" name="document" :id="'document' + document.id" :value="document.id"
                                   v-model="documentId" :="documentIdAtrb">
                            <label :for="'document' + document.id">{{document.document}}</label>
                        </span>
                    </div>
                    <span class="error">{{errors['documentId']}}</span>
                </div>
                <div class="company-input vertical-input">
                    <p class="input-label">Referral Company</p>
                    <div class="checkboxes company input-border">
                        <span v-for="(company, index) in referral_companies">
                            <input type="radio" name="company" :id="'company' + company.id" :value="company.id"
                                   v-model="rcId" :="rcIdAtrb"/>
                            <label :for="'company' + company.id">{{company.name}}</label>
                        </span>
                    </div>
                    <span class="error">{{errors['referralCompanyId']}}</span>
                </div>
                <div class="date-of-assessment-input vertical-input">
                    <p class="input-label">Date of Assessment</p>
                    <div class="date-input">
                        <input aria-label="Date of Assessment" id="doa-input"  type="text" name="doa"
                                       v-model="doAssessment" :="doAssessmentAtrb"/>
                        <span @onclick="document.getElementById('doa-input').focus(); document.getElementById('doa-input').select();">{{formatDate(doAssessment)}}</span>
                    </div>
                    <span class="error">{{errors['dateOfAssessment']}}</span>
                </div>
            </div>

        </fieldset>

        <fieldset class="doc-gen-card">
            <legend class="card-title">Client Information</legend>
            <div class="client-inputs">
                <div class="firstname-input vertical-input">
                    <label for="fname-input" class="vertical-margin">First Name</label>
                    <input id="fname-input" class="input-border" type="text" name="fname" v-model="clFirstName" :="clFirstNameAtrb"/>
                    <span class="error">{{errors['claimant.firstName']}}</span>
                </div>

                <div class="lastname-input vertical-input">
                    <label for="lname-input" class="vertical-margin">Last Name</label>
                    <input aria-label="Last Name" id="lname-input" class="input-border" type="text" name="lname"
                                   v-model="clLastName" :="clLastNameAtrb"/>
                    <span class="error">{{errors['claimant.lastName']}}</span>
                </div>

                <div class="gender-input vertical-input">
                    <!-- <p class="input-label">Gender</p> -->
                    <!-- <div class="horizontal-input input-border"> -->
                    <!--     <input type="radio" id="male" name="gender" value="Male" -->
                    <!--                v-model="clGender" :="clGenderAtrb"/> -->
                    <!--     <label for="male">Male</label><br> -->
                    <!--     <input type="radio" id="female" name="gender" value="Female" -->
                    <!--                v-model="clGender" :="clGenderAtrb"/> -->
                    <!--     <label for="female">Female</label><br> -->
                    <!--     <input type="radio" id="other" name="gender" value="Other" -->
                    <!--                v-model="clGender" :="clGenderAtrb"/> -->
                    <!--     <label for="other">Other</label><br> -->
                    <!-- </div> -->
                  <fieldset class="radio-fieldset">
                        <legend class="input-label">Gender</legend>
                        <div class="horizontal-input input-border">
                            <label class="radio-option">
                                <input type="radio" name="gender" value="Male"
                                       v-model="clGender" v-bind="clGenderAtrb"/>
                                Male
                            </label>
                            <label class="radio-option">
                                <input type="radio" name="gender" value="Female"
                                       v-model="clGender" v-bind="clGenderAtrb"/>
                                Female
                            </label>
                            <label class="radio-option">
                                <input type="radio" name="gender" value="Other"
                                       v-model="clGender" v-bind="clGenderAtrb"/>
                                Other
                            </label>
                        </div>
                      </fieldset>
                    <span class="error">{{errors['claimant.gender']}}</span>
                </div>

                <div class="dob-input vertical-input">
                    <p class="input-label">Date of Birth</p>
                    <div class="date-input">
                        <input aria-label="Date of Birth" id="dob-input" class="input-border" type="text" name="dob"
                                       v-model="clDoBirth" :="clDoBirthAtrb"/>
                        <span>{{formatDate(clDoBirth)}}</span>
                    </div>
                    <span class="error">{{errors['claimant.dateOfBirth']}}</span>
                </div>

                <div class="dol-input vertical-input">
                    <p class="input-label">Date of Loss</p>
                    <div class="date-input">
                        <input aria-label="Date of Loss" id="dol-input" class="input-border" type="text" name="dol"
                                       v-model="clDoLoss" :="clDoLossAtrb"/>
                        <span>{{formatDate(clDoLoss)}}</span>
                    </div>
                    <span class="error">{{errors['claimant.dateOfLoss']}}</span>
                </div>

                <div class="street-input vertical-input">
                    <p class="input-label">Street Address</p>
                    <input aria-label="Street Address" id="street-address-input" class="input-border" type="text" name="address"
                                   v-model="clAddStreetAddress" :="clAddStreetAddressAtrb"/>
                    <span class="error">{{errors['claimant.address.streetAddress']}}</span>
                </div>

                <div class="apt-input vertical-input">
                    <p class="input-label">Apt, Suite, etc</p>
                    <input aria-label="Apt, Suite, etc" id="unit-input" class="input-border" type="text" name="unit"
                                   v-model="clAddUnit" :="clAddUnitAtrb"/>
                    <span class="error">{{errors['claimant.address.unit']}}</span>
                </div>

                <div class="city-input vertical-input">
                    <p class="input-label">City</p>
                    <input aria-label="City" id="city-input" class="input-border" type="text" name="city"
                                   v-model="clAddCity" :="clAddCityAtrb"/>
                    <span class="error">{{errors['claimant.address.city']}}</span>
                </div>

                <div class="province-input vertical-input">
                    <p class="input-label">Province</p>
                    <input aria-label="Province" id="province-input" class="input-border" type="text" name="province"
                                   v-model="clAddProvince" :="clAddProvinceAtrb"/>
                    <span class="error">{{errors['claimant.address.province']}}</span>
                </div>

                <div class="country-input vertical-input">
                    <p class="input-label">Country</p>
                    <input aria-label="Country" id="country-input" class="input-border" type="text" name="country"
                                   v-model="clAddCountry" :="clAddCountryAtrb"/>
                    <span class="error">{{errors['claimant.address.country']}}</span>
                </div>

                <div class="postal-code-input vertical-input">
                    <p class="input-label">Postal Code</p>
                    <input aria-label="Postal Code" id="postal-code-input" class="input-border" type="text" name="postal-code"
                                   v-model="clAddPostalCode" :="clAddPostalCodeAtrb"/>
                    <span class="error">{{errors['claimant.address.postalCode']}}</span>
                </div>

            </div>
        </fieldset>

        <fieldset class="doc-gen-card">
            <legend class="card-title">Insurance Information</legend>
            <div class="insurance-inputs">
                <div class="company-input vertical-input">
                    <p class="input-label">Insurance Company</p>
                    <input aria-label="Insurance Company" id="insurance-company-input" class="input-border" type="text" name="insurance-company" 
                           v-model="insuranceCompany" :="insuranceCompanyAtrb"/>
                    <span class="error">{{errors['insuranceCompany']}}</span>
                </div>

                <div class="adjuster-input vertical-input">
                    <p class="input-label">Adjuster</p>
                    <input aria-label="Adjuster" id="adjuster-input" class="input-border" type="text" name="adjuster" v-model="adjuster" :="adjusterAtrb"/>
                    <span class="error">{{errors['adjuster']}}</span>
                </div>

                <div class="claim-number-input vertical-input">
                    <p class="input-label">Claim Number</p>
                    <input aria-label="Claim Number" id="claim-number-input" class="input-border" type="text" name="claim-number"
                                   v-model="claimNumber" :="claimNumberAtrb"/>
                    <span class="error">{{errors['claimNumber']}}</span>
                </div>
            </div>
        </fieldset>

        <fieldset v-if="includeAC" class="doc-gen-card">
            <legend class="card-title">Attendant Care Benefits</legend>
            <div class="ac-inputs">
                <div class="first-assessment-input vertical-input">
                    <p class="input-label">Is there a previous Form 1?</p>
                    <div class="checkboxes input-border">
                        <span>
                            <input type="radio" id="first-assessment-input-yes" name="first-assessment" :value="true"
                                   v-model="acFirstAssessment" :="acFirstAssessmentAtrb"/>
                            <label for="first-assessment-input-yes">Yes</label>
                        </span>
                        <span>
                            <input type="radio" id="first-assessment-input-no" name="first-assessment" :value="false"
                                   v-model="acFirstAssessment" :="acFirstAssessmentAtrb"/>
                            <label for="first-assessment-input-no">No</label> 
                        </span>
                    </div>
                    <span class="error">{{errors['ac.firstAssessment']}}</span>
                </div>

                <div class="dola-input vertical-input">
                    <p :class="{ 'input-label': acFirstAssessment, 'disabled-input-label': !acFirstAssessment }">Date of Last Assessment</p>
                    <div class="date-input" >
                        <input aria-label="Date of Last Assessment" id="dola-input"  type="text" name="dola" :style="[!acFirstAssessment ? 'color: gray' : '']"
                                       v-model="acDateOfLastAssessment" :="acDateOfLastAssessmentAtrb" :disabled="!acFirstAssessment"/>
                        <span @onclick="document.getElementById('dola-input').focus(); document.getElementById('dola-input').select();"
                              :style="[!acFirstAssessment ? 'color: gray' : '']">
                                {{formatDate(acDateOfLastAssessment)}}
                        </span>
                    </div>
                    <span v-if="acFirstAssessment" class="error">{{errors['ac.dateOfLastAssessment']}}</span>
                </div>

                <div class="monthly-allowance-input vertical-input">
                    <p :class="{ 'input-label': acFirstAssessment, 'disabled-input-label': !acFirstAssessment }">Monthly Allowance</p>
                    <input aria-label="monthly-allowance" id="monthly-allowance-input" class="input-border" type="text" name="monthly-allowance" :style="[!acFirstAssessment ? 'color: gray' : '']"
                                        v-model="acMonthlyAllowance" :="acMonthlyAllowanceAtrb" :disabled="!acFirstAssessment"/>
                    <span v-if="acFirstAssessment" class="error">{{errors['ac.monthlyAllowance']}}</span>
                </div>

            </div>
        </fieldset>

        <fieldset v-if="includeCAT" class="doc-gen-card">
            <legend class="card-title">Catastrophic Impairment</legend>
            <div class="cat-inputs">

                <div class="do-ocf19-input vertical-input">
                    <p class="input-label">Date of OCF19</p>
                    <div class="date-input">
                        <input aria-label="Date of OCF19" id="do-ocf19-input"  type="text" name="do-ocf19"
                                v-model="catDateOfOcf19" :="catDateOfOcf19Atrb"/>
                        <span @onclick="document.getElementById('do-ocf19-input').focus(); document.getElementById('do-ocf19-input').select();">{{formatDate(catDateOfOcf19)}}</span>
                    </div>
                    <span class="error">{{errors['cat.dateOfOcf19']}}</span>
                </div>

                <div class="cat-assessor-input vertical-input">
                    <p class="input-label">Assessor</p>
                    <input aria-label="Assessor" id="cat-assessor-input" class="input-border" type="text" name="cat-assessor"
                            v-model="catAssessor" :="catAssessorAtrb"/>
                    <span class="error">{{errors['cat.assessor']}}</span>
                </div>

            </div>
        </fieldset>

        <fieldset v-if="includeMRB" class="doc-gen-card">
            <legend class="card-title">Medical Rehabilitation Benefits</legend>
            <div class="mrb-inputs">

                <div class="do-ocf18-input vertical-input">
                    <p class="input-label">Date of OCF18</p>
                    <div class="date-input">
                        <input aria-label="Date of OCF18" id="do-ocf18-input"  type="text" name="do-ocf18"
                                v-model="mrbDateOfOcf18" :="mrbDateOfOcf18Atrb"/>
                        <span @onclick="document.getElementById('do-ocf18-input').focus(); document.getElementById('do-ocf18-input').select();">{{formatDate(mrbDateOfOcf18)}}</span>
                    </div>
                    <span class="error">{{errors['mrb.dateOfOcf18']}}</span>
                </div>

                <div class="mrb-assessor-input vertical-input">
                    <p class="input-label">Assessor</p>
                    <input aria-label="Assessor" id="mrb-assessor-input" class="input-border" type="text" name="mrb-assessor"
                            v-model="mrbAssessor" :="mrbAssessorAtrb"/>
                    <span class="error">{{errors['mrb.assessor']}}</span>
                </div>

                <div class="ocf18-amount-input vertical-input">
                    <p class="input-label">Amount of OCF18</p>
                    <input aria-label="Amount of OCF18" id="ocf18-amount-input" class="input-border" type="text" name="ocf18-amount"
                            v-model="mrbAmountOfOcf18" :="mrbAmountOfOcf18Atrb"/>
                    <span class="error">{{errors['mrb.amountOfOcf18']}}</span>
                </div>

            </div>
        </fieldset>

        <div class="horizontal-input" style="justify-content: center; margin-top: 30px;">
            <button class="submit" type="submit">Submit</button>
        </div>

    </form>
</template>

<style lang="scss" scoped>

    @use '../variables';

    .prevent-select {

        -webkit-touch-callout: none; /* iOS Safari */
        -webkit-user-select: none; /* Safari */
        -khtml-user-select: none; /* Konqueror HTML */
        -moz-user-select: none; /* Firefox */
        -ms-user-select: none; /* Internet Explorer/Edge */
        user-select: none; /* Non-prefixed version, currently
                                  supported by Chrome and Opera */
    }

    .assessment-inputs {

        display: grid;
        grid-template-columns: repeat(2, auto);
        grid-template-rows: repeat(3, auto);
        grid-column-gap: 50px;
        grid-row-gap: 15px;
        width: fit-content;

        .assessor-input { grid-area: 1 / 1 / 2 / 3; }
        .document-input { grid-area: 2 / 1 / 3 / 2; }
        .company-input { grid-area: 2 / 2 / 3 / 3; }
        .date-of-assessment-input { grid-area: 3 / 1 / 4 / 2; }

    }

    .client-inputs {

        display: grid;
        grid-template-columns: repeat(3, auto);
        grid-template-rows: repeat(4, auto);
        grid-column-gap: 50px;
        grid-row-gap: 15px;
        width: fit-content;

        .firstname-input { grid-area: 1 / 1 / 2 / 2; }
        .lastname-input { grid-area: 1 / 2 / 2 / 3; }
        .gender-input { grid-area: 1 / 3 / 2 / 4; }

        .dob-input { grid-area: 2 / 1 / 3 / 2; }
        .dol-input { grid-area: 2 / 2 / 3 / 3; }

        .street-input { grid-area: 3 / 1 / 3 / 2; }
        .apt-input { grid-area: 3 / 2 / 3 / 3; }
        .city-input { grid-area: 3 / 3 / 3 / 4; }
        .province-input { grid-area: 4 / 1 / 4 / 2; }
        .country-input { grid-area: 4 / 2 / 4 / 3; }
        .postal-code-input { grid-area: 4 / 3 / 4 / 4; }

    }

    .insurance-inputs {

        display: grid;
        grid-template-columns: repeat(3, auto);
        grid-template-rows: 1fr;
        grid-column-gap: 50px;
        grid-row-gap: 15px;
        width: fit-content;

        .company-input { grid-area: 1 / 1 / 2 / 2; }
        .adjuster-input { grid-area: 1 / 2 / 2 / 3; }
        .claim-number-input { grid-area: 1 / 3 / 2 / 4; }

    }

    .ac-inputs {

        display: grid;
        grid-template-columns: repeat(3, auto);
        grid-template-rows: 1fr;
        grid-column-gap: 50px;
        grid-row-gap: 15px;
        width: fit-content;

        .company-input { grid-area: 1 / 1 / 2 / 2; }
        .adjuster-input { grid-area: 1 / 2 / 2 / 3; }
        .claim-number-input { grid-area: 1 / 3 / 2 / 4; }

    }

    .cat-inputs {

        display: grid;
        grid-template-columns: repeat(2, auto);
        grid-template-rows: 1fr;
        grid-column-gap: 50px;
        grid-row-gap: 15px;
        width: fit-content;

        .do-ocf19-input { grid-area: 1 / 1 / 2 / 2; }
        .cat-assessor-input { grid-area: 1 / 2 / 2 / 3; }

    }

    .mrb-inputs {

        display: grid;
        grid-template-columns: repeat(3, auto);
        grid-template-rows: 1fr;
        grid-column-gap: 50px;
        grid-row-gap: 15px;
        width: fit-content;

        .do-ocf18-input { grid-area: 1 / 1 / 2 / 2; }
        .mrb-assessor-input { grid-area: 1 / 2 / 2 / 3; }
        .ocf18-amount-input { grid-area: 1 / 3 / 2 / 4; }

    }

    .checkboxes {
      display: grid;
      grid-column-gap: 1rem;
      grid-row-gap: 0.5rem;
      grid-template-columns: repeat(4, 1fr);
    }

    .company {
        max-height: 3.5rem;
        overflow: auto;
    }



</style>
