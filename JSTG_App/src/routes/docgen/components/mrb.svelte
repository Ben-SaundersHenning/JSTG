<div>

    <h4>MRB</h4>

    <label for="date">OCF-18 Date:</label>
    <input type="text" id="date" bind:value={mrbData.date} />
    <br>

    <label for="amount">OCF-18 Amount:</label>
    <input type="text" id="amount" bind:value={mrbData.amount} />
    <br>

    <label for="assessor">OCF-18 Assessor:</label>
    <input type="text" id="assessor" bind:value={mrbData.assessor} />
    <br>

</div>
<script>

    import {onMount} from 'svelte'

    onMount(() => {
    });

    export let mrbData = {
        date: "",
        assessor: "",
        amount: ""
    }

</script>
